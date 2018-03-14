using DataTypes.Enums;

namespace IssueBase.Issue
{
    public class IssueStatusModel
    {
        public IssueStatusModel(string name, IssueStatus status)
        {
            Name = name;
            Status = status;
        }

        public int Id { get; set; }
        public IssueStatus Status { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}