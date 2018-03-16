using DataTypes.Enums;

namespace IssueBase.Issue
{
    public class NewIssueModel
    {
        public int LocationId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public IssueStatus Status { get; set; }
        public IssueSeverity Severity { get; set; }
    }
}