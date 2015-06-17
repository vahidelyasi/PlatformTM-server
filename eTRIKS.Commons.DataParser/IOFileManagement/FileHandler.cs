﻿using eTRIKS.Commons.Core.Domain.Model.Templates;
using eTRIKS.Commons.DataParser.DataUtility;
using eTRIKS.Commons.DataAccess.MongoDB;
using eTRIKS.Commons.Service.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eTRIKS.Commons.Core.Domain.Model;
using eTRIKS.Commons.Core.Domain.Interfaces;
using System.Diagnostics;

namespace eTRIKS.Commons.DataParser.IOFileManagement
{
    public class FileHandler
    {
        private TemplateService _templateService;
        //private IRepository<MongoDocument, Guid> _genericMongoRepository;
        //private IServiceUoW _dataContext;

        public FileHandler(TemplateService templateService)
        {
            _templateService = templateService;
            //_dataContext = uoW;
            //_genericMongoRepository = uoW.GetRepository<MongoDocument, Guid>();
        }
        

        public string loadDataFromFile(string dataSource, DataSet ds)
        {
            string status = "NOT CREATED";
            if (dataSource == "DomainTemplate")
            {
                //Remove Column Headers
                DataRow row = ds.Tables[0].Rows[0];
                ds.Tables[0].Rows.Remove(row);

                List<DomainTemplate> domainTemplateList = new List<DomainTemplate>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DomainTemplate dt = new DomainTemplate();
                    dt.Id = ds.Tables[0].Rows[i][0].ToString().Trim();
                    dt.Name = ds.Tables[0].Rows[i][4].ToString().Trim();
                    dt.Class = ds.Tables[0].Rows[i][1].ToString().Trim();
                    dt.Description = ds.Tables[0].Rows[i][6].ToString().Trim();
                    dt.Code = ds.Tables[0].Rows[i][5].ToString().Trim();
                    dt.Structure = ds.Tables[0].Rows[i][7].ToString().Trim();
                    domainTemplateList.Add(dt);
                }
                return _templateService.addDomainTemplate(domainTemplateList);
            }
            else if (dataSource == "DomainVariableTemplate")
            {
                //Remove Column Headers
                DataRow row = ds.Tables[0].Rows[0];
                ds.Tables[0].Rows.Remove(row);
                //Check for FKs
                //checkTemplateDataFK(ds);
                List<DomainVariableTemplate> domainTemplateVariableList = new List<DomainVariableTemplate>();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DomainVariableTemplate dvt = new DomainVariableTemplate();
                    dvt.Id = ds.Tables[0].Rows[i][0].ToString().Trim();
                    dvt.Name = ds.Tables[0].Rows[i][1].ToString().Trim();
                    dvt.Label = ds.Tables[0].Rows[i][2].ToString().Trim();
                    dvt.Description = ds.Tables[0].Rows[i][3].ToString().Trim();
                    dvt.DataType = ds.Tables[0].Rows[i][4].ToString().Trim();
                    dvt.RoleId = _templateService.getOIDOfCVterm(ds.Tables[0].Rows[i][5].ToString().Trim());
                    dvt.UsageId = _templateService.getOIDOfCVterm(ds.Tables[0].Rows[i][6].ToString().Trim());

                    IOUtility iou = new IOUtility();
                    dvt.controlledTerminologyId = iou.processDictionaryIdForTemplateVariable(ds.Tables[0].Rows[i][7].ToString().Trim());
                    dvt.DomainId = ds.Tables[0].Rows[i][8].ToString().Trim();
                    dvt.Order = Convert.ToInt32(ds.Tables[0].Rows[i][9]);
                    domainTemplateVariableList.Add(dvt);
                }
                return _templateService.addDomainTemplateVariables(domainTemplateVariableList);
                //For updating (move to new method)
                //return _templateService.updateDomainTemplateVariables(domainTemplateVariableList);
            }
           
            else if (dataSource == "NOSQL")
            {
                // undo the remove first line
                MongoDbDataRepository ms = new MongoDbDataRepository();
                
                List<MongoDocument> records = new List<MongoDocument>();

                ////string[] headers = ds.Tables[0].Rows[0][0].ToString().Split(new Char[] { ' ', ',', '.', ':', '\t' });
                ////if text file (tab delimited)
                //string[] headers = ds.Tables[0].Columns.ToString().Split(new Char[] { '\t' });
                //for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                //{
                //    record.RecordItems.Clear();
                //    string[] rowElements = ds.Tables[0].Rows[i][0].ToString().Split(new Char[] { '\t' });

                //    for (int j = 0; j < headers.Length; j++)
                //    {
                //        MongoField recordItem = new MongoField();
                //        recordItem.fieldName = headers[j];
                //        recordItem.value = rowElements[j];
                //        record.RecordItems.Add(recordItem);
                //    }
                //    ms.loadDataGeneric(record);
                //}

                //if CSV file
                int count = 0;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    MongoDocument record = new MongoDocument(); //record.fields.Clear();
                    for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                    {
                        MongoField recordItem = new MongoField();
                        recordItem.Name = ds.Tables[0].Columns[j].ToString();
                        recordItem.value = ds.Tables[0].Rows[i][j].ToString();
                        if (recordItem.value != "")
                        {
                            record.fields.Add(recordItem);
                        }
                    }
                    records.Add(record);

                    if (i % 500 == 0)
                    {
                        ms.loadDataGeneric(records);
                        records.Clear();
                        Debug.WriteLine(i + " RECORD(s) SUCCESSFULLY INSERTED");
                    }

                    count = i+1;
                }
                ms.loadDataGeneric(records);

                Debug.WriteLine(count + " RECORD(s) SUCCESSFULLY INSERTED");

                status = "CREATED";
            }
            return status;
        }
    }
}
