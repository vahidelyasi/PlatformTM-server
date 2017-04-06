﻿using System.Collections;
using System.Diagnostics;
using CsvHelper;
using eTRIKS.Commons.Core.Domain.Interfaces;
using eTRIKS.Commons.Core.Domain.Model;
using eTRIKS.Commons.Service.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using eTRIKS.Commons.Core.Domain.Model.DatasetModel;
using eTRIKS.Commons.Core.Domain.Model.DatasetModel.SDTM;
using Microsoft.Extensions.Options;
using eTRIKS.Commons.Service.Configuration;
using Microsoft.IdentityModel.Protocols;
using Remotion.Linq.Parsing;

namespace eTRIKS.Commons.Service.Services
{
    public class FileService
    {
        private readonly IServiceUoW _dataServiceUnit;
        private readonly IRepository<DataFile, int> _fileRepository;
        private readonly IRepository<Project, int> _projectRepository;
        private FileStorageSettings ConfigSettings { get; set; }
        private readonly string _uploadFileDirectory;
        private readonly string _downloadFileDirectory;


        private readonly IRepository<SdtmRow, Guid> _sdtmRepository;
        private readonly IRepository<Observation, int> _observationRepository;

        public FileService(IServiceUoW uoW, IOptions<FileStorageSettings> settings)
        {
            _dataServiceUnit = uoW;
            _fileRepository = uoW.GetRepository<DataFile, int>();
            _projectRepository = uoW.GetRepository<Project, int>();
            ConfigSettings = settings.Value;
            _uploadFileDirectory = ConfigSettings.UploadFileDirectory;
            _downloadFileDirectory = ConfigSettings.DownloadFileDirectory;


            _sdtmRepository = uoW.GetRepository<SdtmRow, Guid>();
            _observationRepository = uoW.GetRepository<Observation, int>();

        }
        public List<FileDTO> GetUploadedFiles(int projectId,string path)
        {
            var files = _fileRepository.FindAll(f => f.ProjectId == projectId && f.Path.Equals(path));
            return files.Select(file => new FileDTO
            {
                FileName = file.FileName,
                dateAdded = file.DateAdded,
                dateLastModified = file.LastModified ?? file.DateAdded,
                icon = "",
                IsDirectory = file.IsDirectory,
                IsLoaded = file.IsLoadedToDB,
                selected = false,
                state = file.State,
                DataFileId = file.Id,
                path = file.Path
            }).ToList();
        }

        public DirectoryInfo AddDirectory(int projectId, string newDir)
        {
            if (Directory.Exists(newDir))
                return new DirectoryInfo(newDir);

            var di = Directory.CreateDirectory(newDir);

            var project = _projectRepository.FindSingle(p => p.Id == projectId);
            if(project ==null)
                return null;

            var file = new DataFile();
            file.FileName = di.Name;
            //file.Path = di.FullName.Substring(di.FullName.IndexOf(projectId));
            file.Path = di.Parent.FullName.Substring(di.Parent.FullName.IndexOf("P-"+projectId));//file.Path.Substring(0,file.Path.LastIndexOf("\\\"))
            file.DateAdded = di.CreationTime.ToString("D");
            file.IsDirectory = true;
            file.ProjectId = project.Id;

            _fileRepository.Insert(file);
            return _dataServiceUnit.Save().Equals("CREATED") ? di : null;
        }

        public void DeleteFile(int fileId)
        {
            var selectFile = _fileRepository.FindSingle(f => f.Id == fileId, new List<string> { "Datasets" });

            bool success = false;
            if (selectFile.State == "LOADED")
            {
                //TODO: if a file is to be unloaded / removed, then all its contents in ALL related datasets are removed
                //TODO: cannot do selective unload/remove of a file
                //HENCE NO NEED TO SPECIFY WHICH DATASETID
                //TODO: NO NEED TO GET datasetId and REMOVE IT FROM PARAMETERS TO UnloadFile
                //var datasetId = selectFile.Datasets.First().DatasetId;
                //UnloadFile(datasetId, fileId);//WHY CALL UnloadFile TWICE?
                success = UnloadFile(fileId);
            }

            //TODO:IF selectFile.State != LOADED, success is still false. Will not be able to delete file
            if (success)
            {
                string fileDir = _uploadFileDirectory;
                string path = fileDir + "\\" + selectFile.Path + "\\" + selectFile.FileName;
                File.Delete(path);
            }
            _fileRepository.Remove(selectFile);
            _dataServiceUnit.Save();
        }

        public bool UnloadFile(int fileId)
        {
            try
            {
                // 1- Delete related observations 
                //TODO: need to account for non-observation datasets, such as samples, subjects and other assay datasets
                _observationRepository.DeleteMany(o => o.DatafileId == fileId);
                // 2- Delete dataset from MongoDB
                _sdtmRepository.DeleteMany(s => s.DatafileId == fileId);

                Debug.WriteLine("RECORD(s) SUCCESSFULLY DELETED FOR DATAFILE:" + fileId);
            }

            // in case an error hapens it returns false for success and therefore the main file would not be deleted. (try method)
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

            //SINCE UNLOAD FILE CAN BE CALLED INDEPENDENTLY OF REMOVING FILE, NEED TO SET STATUS
            var file = _fileRepository.Get(fileId);
            file.State = "UNLOADED";
            _fileRepository.Update(file);
            _dataServiceUnit.Save();

            return true;
        }

        public DataFile AddOrUpdateFile(int projectId, FileInfo fi)
        {
            if (fi == null)
                return null;
            var filePath = fi.DirectoryName.Substring(fi.DirectoryName.IndexOf("P-"+projectId));
            var file = _fileRepository.FindSingle(d => d.FileName.Equals(fi.Name) && d.Path.Equals(filePath) && d.ProjectId == projectId);
            if (file == null)
            {
                var project = _projectRepository.FindSingle(p => p.Id == projectId);
                if (project == null)
                    return null;
                file = new DataFile
                {
                    FileName = fi.Name,
                    DateAdded = fi.CreationTime.ToString("d") + " " + fi.CreationTime.ToString("t"),
                    State = "NEW",
                    Path = fi.DirectoryName.Substring(fi.DirectoryName.IndexOf("P-" + projectId)),
                    IsDirectory = false,
                    ProjectId = project.Id
                };
                _fileRepository.Insert(file);
            }
            else
            {
                file.LastModified = fi.LastWriteTime.ToString("d") + " " + fi.LastWriteTime.ToString("t");
                if (file.IsLoadedToDB)
                    file.State = "UPDATED";
                _fileRepository.Update(file);
            }
            return _dataServiceUnit.Save().Equals("CREATED") ? file : null;
        }

        public List<string> GetDirectories(int projectId)
        {
            var dirs = _fileRepository.FindAll(f => f.IsDirectory.Equals(true) && f.ProjectId == projectId);
            return dirs?.Select(d => d.FileName).ToList();
        }

        public DataTable GetFilePreview(int fileId)
        {

            var file = _fileRepository.Get(fileId);
            var filePath = Path.Combine(file.Path, file.FileName);

            var dataTable = ReadOriginalFile(filePath);

            if (dataTable.Rows.Count > 1000)
                dataTable.Rows.RemoveRange(100, dataTable.Rows.Count - 100);

            if (dataTable.Columns.Count > 40)
                dataTable.Columns.RemoveRange(100, dataTable.Columns.Count - 100);
            dataTable.TableName = file.FileName;

            return dataTable;
        }

        #region IO methods

        public DataTable ReadOriginalFile(string filePath)
        {
            string PATH = _uploadFileDirectory + filePath;
            return readDataFile(PATH);
        }
        
        private DataTable readDataFile(string filePath)
        {
            DataTable dt = new DataTable();

            StreamReader reader = File.OpenText(filePath);
            var parser = new CsvParser(reader);
            string[] header = parser.Read();
            if (!(header.Count() > 1))
            {
                if (header[0].Contains("\t"))
                {
                    parser.Configuration.Delimiter = "\t";
                    header = header[0].Split('\t');
                }
            }


            foreach (string field in header)
            {
                dt.Columns.Add(field.Replace("\"", "").ToUpper(), typeof(string));
            }

            while (true)
            {
                try
                {
                    var row = parser.Read();
                    if (row == null)
                        break;

                    DataRow dr = dt.NewRow();
                    if (row.Length == 0 || row.Length != dt.Columns.Count)
                    {
                        Debug.WriteLine(row.Length + " " + dt.Columns.Count);
                        return null;
                    }

                    for (int i = 0; i < row.Length; i++)
                    {
                        if (row[i] == null)
                            Debug.WriteLine(row);
                        dr[i] = row[i];
                    }
                    dt.Rows.Add(dr);
                }
                catch (System.NullReferenceException e)
                {
                    Debug.WriteLine(e.Message);
                    throw ;
                }
            }
            parser.Dispose();
            reader.Dispose();

            return dt;
        }

        public List<Dictionary<string, string>> getFileColHeaders(string filePath)
        {
            //Parse header of the file
            string PATH = _uploadFileDirectory + filePath;// + studyId + "\\" + fileName;
            StreamReader reader = File.OpenText(PATH);
            string firstline = reader.ReadLine();

            string[] header = null;
            //var parser = new CsvParser(reader);
            if (firstline.Contains("\t"))
                header = firstline.Split('\t');
            else if(firstline.Contains(","))
                header = firstline.Split(',');

            var res = new List<Dictionary<string, string>>();
            for (int i = 0; i < header.Length; i++)
            {
                var r = new Dictionary<string, string>();
                r.Add("colName", header[i].Replace("\"", ""));
                r.Add("pos", i.ToString());
                res.Add(r);
            }
            reader.Dispose();

            return res;
        }

        public FileInfo WriteDataFile(string path, DataTable dt)
        {
            var dirPath = Path.Combine(_downloadFileDirectory, path);
            var di = Directory.CreateDirectory(dirPath);
            if(!di.Exists) return null;
            var filePath = Path.Combine(dirPath, dt.TableName + ".csv");


            StreamWriter writer = File.CreateText(filePath);

            var headerValues = dt.Columns.Cast<DataColumn>()
                .Select(column => QuoteValue(column.ColumnName));

            writer.WriteLine(string.Join(",", headerValues));

            foreach (DataRow row in dt.Rows)
            {
                var items = row.Values.Cast<object>().Select(o => QuoteValue(o.ToString()));
                writer.WriteLine(string.Join(",", items));
            }
            writer.Flush();
            writer.Dispose();
            return new FileInfo(filePath);
        }

        private static string QuoteValue(string value)
        {
            return String.Concat("\"",
            value.Replace("\"", "\"\""), "\"");
        }

        #endregion

        //private Hashtable getHashtable(DataTable sdtmTable)
        //{


        //    //if (sdtmTable.Rows.Count > 10000)
        //    //    sdtmTable.Rows.RemoveRange(100, sdtmTable.Rows.Count - 100);


        //    //if (sdtmTable.Columns.Count > 50)
        //    //    sdtmTable.Columns.RemoveRange(10, sdtmTable.Columns.Count - 10);


        //    var ht = new Hashtable();
        //    var headerList = new List<Dictionary<string, string>>();
        //    foreach (var col in sdtmTable.Columns.Cast<DataColumn>())
        //    {
        //        var header = new Dictionary<string, string>
        //        {
        //            {"data", col.ColumnName.ToLower()},
        //            {"title", col.ColumnName}
        //        };
        //        headerList.Add(header);
        //    }
        //    ht.Add("header", headerList);
        //    ht.Add("data", sdtmTable.Rows);

        //    return ht;
        //}


        //public void tempmethod()
        //{
        //    DataTable usubjids = ReadOriginalFile("temp/CRC305Dusubjids.csv");
        //    //DataTable cytofSamples = ReadOriginalFile("temp/CyTOFsamples.csv");
        //    DataTable Samples = ReadOriginalFile("temp/BS_ic.csv");
        //    //DataTable FACSSamples = ReadOriginalFile("temp/FACSsamples_v1.csv");
        //    //luminexSamples.TableName = "luminexSamples";

        //    //Samples.Columns.Add("USUBJID");
        //    List<string> subjidlist = new List<string>();
        //    Dictionary<string, string> idmap = new Dictionary<string, string>();

        //    foreach (DataRow row in usubjids.Rows)
        //    {
        //        string[] id = row[0].ToString().Split('-');
        //        idmap.Add(id[2],row[0].ToString());
        //    }

        //    foreach (DataRow row in Samples.Rows)
        //    {
        //        string subjId = row["donor"].ToString();
        //        //string newsubjid = subjidlist.Find(d => d.EndsWith(subjId));
        //        if(subjId == "N/A")
        //            continue;
        //        string newsubjid = idmap[subjId];
        //        row["USUBJID"] = newsubjid;
        //    }
        //    string path = ConfigurationManager.AppSettings["FileDirectory"];
        //    StreamWriter writer = File.CreateText(path + "temp\\BS.csv");

        //    IEnumerable<String> headerValues = Samples.Columns.Cast<DataColumn>()
        //        .Select(column => QuoteValue(column.ColumnName));

        //    writer.WriteLine(String.Join(",", headerValues));
        //    IEnumerable<String> items;

        //    foreach (DataRow row in Samples.Rows)
        //    {
        //        items = row.ItemArray.Select(o => QuoteValue(o.ToString()));
        //        writer.WriteLine(String.Join(",", items));
        //    }
        //    writer.Flush();
        //    writer.Dispose();
        //}

        /**
         *To gather one column we need the following params
         *- identifier columns (columns that will remain as is
         *- gather columns (those that will be pivoted to long format
         *- name of the key column
         *- name of the value column
         */
        //public void getLongFormat()
        //{
        //    DataTable wideDataTable = ReadOriginalFile("temp/CyTOFdata_v2.csv");
        //    DataTable longDataTable = new DataTable();

        //    List<string> ids = new List<string>() { "SAMPLEID","POP","COUNT", "PERTOT"};
        //    List<string> gatherColumns = new List<string>();
        //    int gatherColumnsFrom = 7;
        //    int gatherColumnsTo = 111;

        //    List<int> countColumns = new List<int>(){1,10,19,28};

        //    //Retrieve dataset template for the long format file
        //    //identify key column and value Column
        //    string keyColumn = "OBSMEA", valueColumn = "OBSVALUE",
        //        featureColumn = "FEAT", domainColumn = "DOMAIN";

        //    //1- Create new table from the identifier columns + the new columns
        //    longDataTable.Columns.Add(domainColumn);
        //    foreach (var idCol in ids )
        //    {
        //        longDataTable.Columns.Add(idCol);
        //    }
        //    //longDataTable.Columns.Add(popColumn);
        //    //longDataTable.Columns.Add(countColumn);
        //    longDataTable.Columns.Add(featureColumn);
        //    longDataTable.Columns.Add(keyColumn);
        //    longDataTable.Columns.Add(valueColumn);

        //    foreach (DataRow inRow in wideDataTable.Rows)
        //    {


        //        for (int i = gatherColumnsFrom; i <= gatherColumnsTo; i++)
        //        {
        //            DataRow longDataRow = longDataTable.NewRow();
        //            foreach (var idCol in ids)
        //            {
        //                longDataRow[idCol] = inRow[idCol];
        //            }
        //            string[] keyValue = wideDataTable.Columns[i].ToString().Split('.');
        //            longDataRow[keyColumn] = keyValue[0];
        //            longDataRow[valueColumn] = inRow[i];
        //            longDataRow[featureColumn] = keyValue[1];
        //            longDataRow[domainColumn] = "CY";

        //            longDataTable.Rows.Add(longDataRow);
        //        }
        //        //foreach (DataColumn col in inputDataTable.Columns)
        //        //{

        //        //}
        //    }
        //   // var fileInfo = writeDataFile("temp/CyTOFdata_long.csv", longDataTable);
        //    string path = ConfigurationManager.AppSettings["FileDirectory"];
        //    StreamWriter writer = File.CreateText(path+"temp\\CyTOFdata_long.csv");

        //    IEnumerable<String> headerValues = longDataTable.Columns.Cast<DataColumn>()
        //        .Select(column => QuoteValue(column.ColumnName));

        //    writer.WriteLine(String.Join(",", headerValues));
        //    IEnumerable<String> items;

        //    foreach (DataRow row in longDataTable.Rows)
        //    {
        //        items = row.ItemArray.Select(o => QuoteValue(o.ToString()));
        //        writer.WriteLine(String.Join(",", items));
        //    }
        //    writer.Flush();
        //    writer.Dispose();
        //}

        //public void getLongFormat2()
        //{
        //    DataTable wideDataTable = ReadOriginalFile("temp/FACSdata_v2.csv");
        //    DataTable longDataTable = new DataTable();

        //    //List<string> ids = new List<string>() { "SAMPLEID","POP","COUNT", "PERTOT"};
        //    List<string> ids = new List<string>() { "SAMPLEID" };
        //    List<string> gatherColumns = new List<string>();
        //    int gatherColumnsFrom = 7;
        //    int gatherColumnsTo = 111;



        //    List<int> countColumns = new List<int>() { 1, 10, 19, 28 };

        //    //Retrieve dataset template for the long format file
        //    //identify key column and value Column
        //    string countColumn = "COUNT", keyColumn = "OBSMEA", valueColumn = "OBSVALUE",
        //        featureColumn = "FEAT", domainColumn = "DOMAIN", popColumn = "POPULATION";



        //    //1- Create new table from the identifier columns + the new columns
        //    longDataTable.Columns.Add(domainColumn);
        //    foreach (var idCol in ids)
        //    {
        //        longDataTable.Columns.Add(idCol);
        //    }
        //    longDataTable.Columns.Add(popColumn);
        //    longDataTable.Columns.Add(countColumn);
        //    longDataTable.Columns.Add(featureColumn);
        //    longDataTable.Columns.Add(keyColumn);
        //    longDataTable.Columns.Add(valueColumn);

        //    foreach (DataRow inRow in wideDataTable.Rows)
        //    {
        //        for(int k=0; k<countColumns.Count;k++)
        //        {
        //            for (int i = countColumns[k] + 1; k+1==countColumns.Count?i<inRow.ItemArray.Length:i < countColumns[k+1]; i++)
        //            {
        //                DataRow longDataRow = longDataTable.NewRow();
        //                foreach (var idCol in ids)
        //                {
        //                    longDataRow[idCol] = inRow[idCol];
        //                }
        //                string[] popCountKeyValue = wideDataTable.Columns[countColumns[k]].ToString().Split('.');

        //                longDataRow[popColumn] = popCountKeyValue[0];
        //                longDataRow[countColumn] = inRow[countColumns[k]];

        //                string[] keyValue = wideDataTable.Columns[i].ToString().Split('.');
        //                longDataRow[keyColumn] = keyValue[1];
        //                longDataRow[valueColumn] = inRow[i];
        //                longDataRow[featureColumn] = keyValue[2];
        //                longDataRow[domainColumn] = "CY";

        //                longDataTable.Rows.Add(longDataRow);
        //            }
        //        }


        //        //foreach (DataColumn col in inputDataTable.Columns)
        //        //{

        //        //}
        //    }
        //    // var fileInfo = writeDataFile("temp/CyTOFdata_long.csv", longDataTable);
        //    string path = ConfigurationManager.AppSettings["FileDirectory"];
        //    StreamWriter writer = File.CreateText(path + "temp\\FACSdata_long.csv");

        //    IEnumerable<String> headerValues = longDataTable.Columns.Cast<DataColumn>()
        //        .Select(column => QuoteValue(column.ColumnName));

        //    writer.WriteLine(String.Join(",", headerValues));
        //    IEnumerable<String> items;

        //    foreach (DataRow row in longDataTable.Rows)
        //    {
        //        items = row.ItemArray.Select(o => QuoteValue(o.ToString()));
        //        writer.WriteLine(String.Join(",", items));
        //    }
        //    writer.Flush();
        //    writer.Dispose();
        //}

        public FileDTO GetFileDTO(int fileId)
        {
            var file = _fileRepository.Get(fileId);

            var dto = new FileDTO()
            {
              FileName = file.FileName,
                dateAdded = file.DateAdded,
            dateLastModified = file.LastModified ?? file.DateAdded,
            icon = "",
            IsDirectory = file.IsDirectory,
            selected = false,
            state = file.State,
            DataFileId = file.Id,
            path = file.Path
        };
            return dto;
        }

        public string GetFullPath(string projectId, string subdir)
        {
            return Path.Combine(_uploadFileDirectory, "P-" + projectId, subdir);
        }
    }
}
