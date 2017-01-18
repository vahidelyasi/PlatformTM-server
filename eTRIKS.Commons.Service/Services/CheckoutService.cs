﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eTRIKS.Commons.Core.Domain.Interfaces;
using eTRIKS.Commons.Core.Domain.Model;
using eTRIKS.Commons.Core.Domain.Model.DatasetModel.SDTM;
using eTRIKS.Commons.Core.Domain.Model.Users.Datasets;
using eTRIKS.Commons.Core.Domain.Model.Users.Queries;
using eTRIKS.Commons.Service.DTOs;

namespace eTRIKS.Commons.Service.Services
{
    public class CheckoutService
    {
        private IServiceUoW _dataContext;

        private readonly IRepository<CombinedQuery, Guid> _combinedQueryRepository;
        private readonly IRepository<UserDataset, Guid> _userDatasetRepository;
        private ExportService _exportService;

        public CheckoutService(IServiceUoW uoW, ExportService exportService)
        {
            _dataContext = uoW;
            _combinedQueryRepository = uoW.GetRepository<CombinedQuery, Guid>();
            _userDatasetRepository = uoW.GetRepository<UserDataset, Guid>();
            _exportService = exportService;
        }

        public List<UserDataset> CreateCheckoutDatasets(string queryIdStr, string userId)
        {
            Guid queryId;
            if (!Guid.TryParse(queryIdStr, out queryId))
                return null;
            var query = _combinedQueryRepository.Get(queryId);

            var phenoDataset = new UserDataset();
            phenoDataset.Id = Guid.NewGuid();
            phenoDataset.OwnerId = userId;
            phenoDataset.ProjectId = query.ProjectId;
            phenoDataset.Type = "PHENO";
            phenoDataset.Name = "Phenotypes";
            //CREATE DATAFIELDS

            //TEMP //SHOULD ADD SUBJECTID and UNIQUE SUBJECT ID to CHARACTERISTICS/CHARACTERISTICS_OBJ
            phenoDataset.Fields.Add(new DatasetField()
            {
                FieldName = "Subject[UniqueId]",
                ColumnHeader = "subjectid",
                ColumnHeaderIsEditable = false
            });

            //ADD DESIGN ELEMENT FIELDS (STUDY, VISIT, ARM...etc)
            phenoDataset.Fields.AddRange(query.DesignElements.Select(qObj => new DatasetField()
            {
                QueryObject = qObj,
                QueryObjectType = qObj.TermName, //TEMP should consider to add type to obsquery if used as generic query
                ColumnHeader = qObj.TermName
            }));

            //ADD SUBJECT CHARACTERISTICS (AGE, RACE, SEX ...etc) 
            phenoDataset.Fields.AddRange(query.SubjectCharacteristics.Select(qObj => new DatasetField()
            {
                QueryObject = qObj,
                QueryObjectType = nameof(SubjectCharacteristic),
                ColumnHeader = qObj.ObservationName
            }));

            //ADD CLINICAL OBSERVATIONS
            phenoDataset.Fields.AddRange(query.ClinicalObservations.Select(qObj => new DatasetField()
            {
                QueryObject = qObj,
                QueryObjectType = nameof(SdtmRow),
                ColumnHeader = qObj.ObservationName
            }));

            phenoDataset.Fields.AddRange(query.GroupedObservations.Select(gObs => new DatasetField()
            {
                QueryObject = gObs,
                QueryObjectType = nameof(SdtmRow),
                ColumnHeader = gObs.ObservationName
            }));
            _userDatasetRepository.Insert(phenoDataset);
            _dataContext.Save();

            return new List<UserDataset>() {phenoDataset};
        }

        public DataTable ExportDataset(string datasetId)
        {
            var dataset = _userDatasetRepository.FindSingle(d => d.Id == Guid.Parse(datasetId));
            var projectId = dataset.ProjectId;

            var exportData = _exportService.GetDatasetContent(projectId, dataset);

            var dt = _exportService.GetDatasetTable(exportData,dataset);
            dt.TableName = dataset.Name;

            return dt;
            
        }

        public string downloadDatasets(DataTable dtTable)
        {
            StringBuilder result = new StringBuilder();
            if (dtTable.Columns.Count != 0)
            {
                foreach (DataColumn col in dtTable.Columns)
                {
                    result.Append(col.ColumnName + ',');
                }
                result.Append("\r\n");
                foreach (DataRow row in dtTable.Rows)
                {
                    foreach (DataColumn column in dtTable.Columns)
                    {
                        result.Append(row[column].ToString() + ',');
                    }
                    result.Append("\r\n");
                }
            }
            return result.ToString();
        }

    }
}
