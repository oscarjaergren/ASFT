using System;
using System.Collections.Generic;
using DataTypes;
using DataTypes.Enums;

namespace IssueBase.Issue
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
        public string Name { get; set; }

        public bool IssueNeedSync = true; // Issue is saved localy, But not sent to server. Need syncing
        public bool IsNewIssue;
        public bool IssueChanged;

        public NewIssueModel CreateNewIssueModel()
        {
            NewIssueModel issue = new NewIssueModel
            {
                LocationId = LocationId,
                Title = Title,
                Description = Description,
                Longitude = Longitude,
                Latitude = Latitude,
                Status = Status,
                Severity = Severity
            };
            return issue;
        }

        public IssueModel CreateUpdatedIssueViewModel()
        {
            IssueModel issue = new IssueModel
            {
                Id = Id,
                LocationId = LocationId,
                Description = Description,
                Longitude = Longitude,
                Latitude = Latitude,
                Status = Status,
                Severity = Severity
            };
            issue.CreatedBy = issue.CreatedBy;
            return issue;
        }

        public List<IssueStatusModel> PossibleStatusValues
        {
            get
            {
                var items = new List<IssueStatusModel>
                {
                    new IssueStatusModel("statusUnresolved.png", IssueStatus.Unresolved),
                    new IssueStatusModel("statusInProgress.png", IssueStatus.InProgress),
                    new IssueStatusModel("statusDone.png", IssueStatus.Done)
                };

                return items;
            }
        }

        public List<IssueSeverityModel> PossibleSeverityValues
        {
            get
            {
                var items = new List<IssueSeverityModel>
                {
                    new IssueSeverityModel("severity_5.png", IssueSeverity.Highest),
                    new IssueSeverityModel("severity_4.png", IssueSeverity.High),
                    new IssueSeverityModel("severity_3.png", IssueSeverity.Medium),
                    new IssueSeverityModel("severity_2.png", IssueSeverity.Low),
                    new IssueSeverityModel("severity_1.png", IssueSeverity.Lowest)
                };
                return items;
            }
        }


        public override string ToString()
        {
            return Name;
        }
    }
}