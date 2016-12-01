﻿using System.Collections.Generic;
using eTRIKS.Commons.Service.Services;
using System.Collections;
using System.Threading.Tasks;
using eTRIKS.Commons.Service.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eTRIKS.Commons.WebAPI.Controllers
{
    [Route("api/apps/explore")]
    //[Authorize]
    public class DataExplorerController : Controller
    {
        private DataExplorerService _dataService;

        public DataExplorerController(DataExplorerService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet("projects/{projectId}/subjcharacteristics/browse")]
        public List<ObservationRequestDTO> getSubjectCharacteristics(int projectId)
        {
            return _dataService.GetSubjectCharacteristics(projectId);
        }

        [HttpPost("projects/{projectId}/subjects/search")]
        public  Hashtable GetSubjectData(int projectId, [FromBody] List<ObservationRequestDTO> requestedSCs)
        {
            return  _dataService.GetSubjectData(projectId, requestedSCs);
        }

        [HttpPost("projects/{projectId}/observations/clinical/search")]
        public Hashtable GetObservations(int projectId, [FromBody] List<ObservationRequestDTO> observations)
        {
            return _dataService.GetObservationsData(projectId, observations);
        }

        [HttpPost("projects/{projectId}/observations/clinical/group")]
        public ObservationNode GroupObservations(int projectId, [FromBody] List<ObservationRequestDTO> observations)
        {
            return _dataService.GroupObservations(projectId, observations);
        }

        [HttpGet("projects/{projectId}/observations/clinical/browse")]
        public async Task<IEnumerable<ClinicalDataTreeDTO>> getClinicalTree(int projectId)
        {
            return await _dataService.GetClinicalObsTree(projectId);
        }

    }
}
