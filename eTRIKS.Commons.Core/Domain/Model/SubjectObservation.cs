﻿using eTRIKS.Commons.Core.Domain.Model.Base;
using eTRIKS.Commons.Core.Domain.Model.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eTRIKS.Commons.Core.Domain.Model
{
    public class SubjectObservation : Identifiable<Guid>
    {
        public string SubjectId { get; set; }
        public string StudyId { get; set; }
        public string Name { get; set; }
        public string Class { get; set; } //Findings
        public string DomainCode { get; set; }
        public string Group { get; set; } //null //shuold be variable not string?
        public string Subgroup { get; set; } //null 
        public string Visit { get; set; }
        public int VisitNum { get; set; }
        public Dictionary<string, string> qualifiers { get; set; }
        public Dictionary<string, string> timings { get; set; }
        public AbsoluteTimePoint ObDateTime { get; set; } //Date of Collection / --DTC
        public RelativeTimePoint ObsStudyDay { get; set; } //--DY
        public RelativeTimePoint ObsStudyTimePoint { get; set; } //--TPT
        public TimeInterval ObsInterval { get; set; }

        public class ObsQualifier
        {
            enum type
            {

            }
            public string fieldName { get; set; }
            public string value { get; set; }
        }



        
    }
}
