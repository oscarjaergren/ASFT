using DataTypes;
using DataTypes.Enums;

namespace IssueBase.Issue
{
    public class IssueSeverityModel
    {
        public IssueSeverityModel(string name, IssueSeverity severity)
        {
            Name = name;
            Severity = severity;
        }

        public int Id { get; set; }
        public IssueSeverity Severity { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}