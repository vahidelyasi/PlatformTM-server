//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eTRIKS.Commons.Persistence.ModelFirst
{
    using System;
    using System.Collections.Generic;
    
    public partial class Derived_Variable_Method_TAB
    {
        public string derivedVariableId { get; set; }
        public string methodDescription { get; set; }
        public string type { get; set; }
    
        public virtual Variable_Def_TAB Variable_Def_TAB { get; set; }
    }
}