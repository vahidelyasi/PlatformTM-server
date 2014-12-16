﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using eTRIKS.Commons.Core.Domain.Model;
using System.Reflection;

namespace eTRIKS.Commons.DataParser.IOFileManagement
{
    public class IOUtility
    {
        public List<string> getDataSourceColumns(string dataSource)
        {

            //Type myType = Type.GetType("eTRIKS.Commons.Core.Domain.Model." + dataSource);

            Type myType = typeof(Activity);
            FieldInfo[] calssFields = myType.GetFields();
            return null;
            //object created = Activator.CreateInstance(myType);
            //return created.GetType().GetProperties().Select(a => a.Name).ToList();
        }

        public DataTable convertByteArraytoExcelDataTable(byte[] file)
        {
            DataTable inputExcelFile = new DataTable();
            using (MemoryStream stream = new MemoryStream(file))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                inputExcelFile = (DataTable)bformatter.Deserialize(stream);
            }
            return inputExcelFile;
        }

        public DataSet convertByteArraytoExcelDataSet(byte[] file)
        {
            DataSet inputExcelFile = new DataSet();
            using (MemoryStream stream = new MemoryStream(file))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                inputExcelFile = (DataSet)bformatter.Deserialize(stream);
            }
            return inputExcelFile;
        }

        static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        // Needed to decode the URI data sent
        public string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }

        public void saveFile(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }

        public DataSet readCSVFileContents(string fileName, string mapping)
        {
            string fileLocation = @"C:\temp\" + fileName;

            OleDbConnection connection = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                                           Path.GetDirectoryName(fileLocation) +
                                               "; Extended Properties = \"text;HDR=NO;FMT=Delimited\"");
            connection.Open();
            OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT " + mapping + " FROM " + fileName, connection);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            connection.Close();
            return ds;
        }

        public DataSet readTabDelimitedFileContents(string fileName, string mapping)
        {
            string fileLocation = @"C:\temp\" + fileName;

            OleDbConnection connection = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " +
                                           Path.GetDirectoryName(fileLocation) +
                                               "; Extended Properties = \"text;HDR=NO;FMT=TabDelimited\"");
            connection.Open();
            OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT " + mapping + " FROM " + fileName, connection);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            connection.Close();

            return ds;
        }

        public DataSet readExcelFileContents(string fileName, string page, string mapping)
        {
            string fileLocation = @"C:\temp\" + fileName;
            var connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 12.0;IMEX=1;HDR=NO;TypeGuessRows=0;ImportMixedTypes=Text\"";
            OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT " + mapping + " FROM [" + page + "$]", connectionString);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            return ds;
        }

        public DataTable readExcelFilePages(string fileName)
        {
            string fileLocation = @"C:\temp\" + fileName;
            OleDbConnection connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileLocation + ";Extended Properties='Excel 12.0 xml;HDR=YES;'");
            connection.Open();
            DataTable pages = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            return pages;
        }
    }
}
