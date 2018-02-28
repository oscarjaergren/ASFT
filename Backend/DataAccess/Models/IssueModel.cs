using System;
using DataTypes;

namespace DataAccess.Models
{
	public class IssueModel
	{
		public int LocationId { get; set; }
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public double Longitude { get; set; }
		public double Latitude { get; set; }
		public IssueStatus Status { get; set; }
		public IssueSeverity Severity { get; set; }
		public DateTime Created { get; set; }
		public DateTime Edited { get; set; }

    public string CreatedBy { get; set; }

		public bool IsClosed
		{
			get { return Status == IssueStatus.Done; }
		}
	}
}
