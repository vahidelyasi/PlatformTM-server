﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eTRIKS.Commons.Core.Domain.Model;
using eTRIKS.Commons.Core.Domain.Model.ControlledTerminology;

namespace eTRIKS.Commons.Service.DTOs
{
    class DatasetDTO
    {
        public string Name { get; set; }
        public string Class { get; set; }
        public string Description { get; set; }

        public List<DatasetVariableDTO> variables { get; set; }
    }

    class DatasetVariableDTO
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        //public string DataType { get; set; }
        //public Nullable<bool> IsCurated { get; set; }
        //public CVterm VariableType { get; set; }
        //public string VariableTypeId { get; set; }
        //public CVterm Role { get; set; }
        //public string RoleId { get; set; }
        //public Study study { get; set; }
        //public string StudyId { get; set; }

        public Nullable<int> OrderNumber { get; set; }
        public Nullable<bool> IsRequired { get; set; }
        public Nullable<int> KeySequence { get; set; }
        public bool isSelected { get; set; }
        public string CVdictionary { get; set; }
        //public DerivedMethod DerivedVariableProperties { get; set; }
        //public string DerivedVariablePropertiesId { get; set; }
    }
}