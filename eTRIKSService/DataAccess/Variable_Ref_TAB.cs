//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eTRIKSService.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class Variable_Ref_TAB
    {
        public string variableId { get; set; }
        public string activityDatasetId { get; set; }
        public Nullable<int> orderNo { get; set; }
        public Nullable<bool> mandatory { get; set; }
        public Nullable<int> keySequence { get; set; }
    
        public virtual Activity_Dataset_TAB Activity_Dataset_TAB { get; set; }
        public virtual Variable_Def_TAB Variable_Def_TAB { get; set; }
    }
}
