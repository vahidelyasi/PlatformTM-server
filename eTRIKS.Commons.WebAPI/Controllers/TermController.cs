﻿using System;
using System.Collections.Generic;
using eTRIKS.Commons.Core.Domain.Model.ControlledTerminology;
using eTRIKS.Commons.Service.DTOs;
using eTRIKS.Commons.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace eTRIKS.Commons.WebAPI.Controllers
{
    public class TermController : Controller
    {
        private CVtermService _cvtermService;

        public TermController(CVtermService cvTermService)
        {
            _cvtermService = cvTermService;
        }


        [HttpGet]
        [Route("api/terms/assay/measurementTypes")]
        public List<AssayDefTermsDTO> GetAssayMeasurementTypes()
        {

            return _cvtermService.GetAssayDefTerms();
        }
    }
}