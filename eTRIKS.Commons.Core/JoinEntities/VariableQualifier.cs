﻿using eTRIKS.Commons.Core.Domain.Model;
using eTRIKS.Commons.Core.Domain.Model.DatasetModel;
using eTRIKS.Commons.Core.Domain.Model.DesignElements;

namespace eTRIKS.Commons.Core.JoinEntities
{
    public class VariableQualifier
    {
        public int VariableId {get; set; }
        public VariableDefinition Variable { get; set; }

        public int QualifierId { get; set; }
        public VariableDefinition Qualifier { get; set; }
    }
}