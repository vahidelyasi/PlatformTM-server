﻿using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformTM.Services.DTOs;
using PlatformTM.Services.DTOs.Explorer;
using PlatformTM.Services.Services;

namespace PlatformTM.API.Controllers
{
    [Route("apps/explore")]
    public class DataExplorerController : Controller
    {
        private readonly DataExplorerService _explorerService;
        private readonly QueryService _queryService;
        private readonly ProjectService _projectService;

        public DataExplorerController(DataExplorerService explorerService, QueryService queryService, ProjectService projectService)
        {
            _explorerService = explorerService;
            _queryService = queryService;
            _projectService = projectService;
        }

        [HttpGet("projects/{projectId}/subjcharacteristics/browse")]
        public IActionResult GetSubjectCharacteristics(int projectId)
        {
            var subjChars = _explorerService.GetSubjectCharacteristics(projectId);
            if (subjChars != null)
                return Ok(subjChars);
            return NotFound();
        }
        
        [HttpPost("projects/{projectId}/saveQuery")]
        public IActionResult SaveQuery(int projectId, [FromBody] CombinedQueryDTO cdto )
       {
          var userId = User.FindFirst(ClaimTypes.UserData).Value;
          var savedQuery =  _queryService.SaveQuery(cdto, userId, projectId);
            
            if (savedQuery != null)
                return new CreatedAtRouteResult("GetSavedQuery", new { projectId = projectId, queryId = savedQuery.Id.ToString() }, savedQuery);
            
            return new StatusCodeResult(StatusCodes.Status409Conflict);
       }


        [HttpGet("projects/{projectId}/queries/{queryId}", Name = "GetSavedQuery")]
        public IActionResult GetSavedQuery(int projectId, string queryId)
        {
            var userId = User.FindFirst(ClaimTypes.UserData).Value;
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();
            var query = _queryService.GetSavedCombinedQuery(projectId, userId,queryId);
            if(query != null)
                return Ok(query);
            return NotFound();
        }

        [HttpGet("projects/{projectId}/queries/browse", Name = "")]
        public IActionResult GetSavedQueries(int projectId) 
        {
            var userId = User.FindFirst(ClaimTypes.UserData).Value;
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();
            var queries = _projectService.GetProjectSavedQueries(projectId, userId);
            if (queries != null)
                return Ok(queries);
            return NotFound();
        }

        /*
        [Route("projects/{projectId}/UpdateQueries")]
        [HttpGet]
        //public IEnumerable<CombinedQueryDTO> Get()
        public List<CombinedQuery> UpdateQueries(CombinedQueryDTO cdto, int projectId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (!User.Identity.IsAuthenticated)
                return null;
            return _explorerService.UpdateQueries(cdto, projectId, userId);
        }
        */
         
        [HttpPost("projects/{projectId}/subjects/search")]
        public  DataTable GetSubjectData(int projectId, [FromBody] List<ObservationRequestDTO> requestedSCs)
        {
            return  _explorerService.GetSubjectData(projectId, requestedSCs);
        }

        [HttpPost("projects/{projectId}/observations/clinical/search")]
        public Hashtable GetObservations(int projectId, [FromBody] List<ObservationRequestDTO> observations)
        {
            return _explorerService.GetObservationsData(projectId, observations);
        }

        [HttpPost("projects/{projectId}/observations/clinical/group")]
        public ObservationNode GroupObservations(int projectId, [FromBody] List<ObservationRequestDTO> observations)
        {
            return _explorerService.GroupObservations(projectId, observations);
        }
        [HttpPost("projects/{projectId}/observations/clinical/{obsId}/qualifiers")]
        public List<ObservationRequestDTO> GetObservationQualifiers(int projectId, [FromBody] ObservationRequestDTO obsReq)
        {
            return _explorerService.GetObsQualifierRequests(projectId, obsReq);
        }

        [HttpGet("projects/{projectId}/observations/clinical/browse")]
        public async Task<ClinicalExplorerDTO> GetClinicalTree(int projectId)
        {
            return await _explorerService.GetClinicalObsTree(projectId);
        }

        [HttpGet("projects/{projectId}/assays/browse")]
        public List<AssayBrowserDTO> GetAssays(int projectId)
        {
            return _explorerService.GetProjectAssays(projectId);
        }

        [HttpPost("projects/{projectId}/assays/{assayId}/samples/search")]
        public DataTable GetAssaySamples(int projectId, int assayId, [FromBody] List<ObservationRequestDTO> characteristics)
        {
            return _explorerService.GetSampleDataForAssay(assayId, characteristics);
        }
    }
}
