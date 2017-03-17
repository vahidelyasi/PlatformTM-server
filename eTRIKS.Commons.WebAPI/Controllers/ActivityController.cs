﻿using eTRIKS.Commons.Core.Domain.Model;
using eTRIKS.Commons.Service.Services;
using System;
using eTRIKS.Commons.Service.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace eTRIKS.Commons.WebAPI.Controllers
{
    [Route("activities")]
    public class ActivityController : Controller
    {
        private readonly ActivityService _activityService;
        private AssayService _assayService;

        public ActivityController(ActivityService activityService, AssayService assayService)
        {
            _activityService = activityService;
            _assayService = assayService;
        }

        [HttpGet("{activityId}", Name = "GetActivityById")]
        public IActionResult GetActivity(int activityId)
        {
            var activity = _activityService.GetActivity(activityId);
            if(activity != null)
                return Ok(_activityService.GetActivity(activityId));
            return NotFound();
        }

        [HttpPost]
        public IActionResult AddActivity([FromBody] ActivityDTO activityDTO)
        {
            ActivityDTO addedActivity =null;
            if(!activityDTO.isAssay)
                addedActivity = _activityService.AddActivity(activityDTO);

            if (addedActivity != null)
                return new CreatedAtRouteResult("GetActivityById", new { activityId = addedActivity.Id }, addedActivity);

            return new StatusCodeResult(StatusCodes.Status409Conflict);

        }

        [HttpPut("{activityId}")]
        public IActionResult UpdateActivity(int activityId, [FromBody] ActivityDTO activityDTO)
        {
            try{
                _activityService.UpdateActivity(activityDTO, activityId);
                return new AcceptedAtRouteResult("GetActivityById", new { activityId = activityDTO.Id }, activityDTO);
            }
            catch (Exception e)
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
        }

        
       
    }
}
