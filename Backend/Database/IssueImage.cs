//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class IssueImage
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int IssueId { get; set; }
        public System.DateTime Created { get; set; }
    
        public virtual Issue Issue { get; set; }
    }
}
