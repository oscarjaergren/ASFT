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
    
    public partial class Issue
    {
        public Issue()
        {
            this.Images = new HashSet<IssueImage>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public short Status { get; private set; }
        private short Severity { get; set; }
        public int LocationId { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime Edited { get; set; }
        public string CreatedBy { get; set; }
    
        public virtual ICollection<IssueImage> Images { get; set; }
        public virtual Location Location { get; set; }
    }
}
