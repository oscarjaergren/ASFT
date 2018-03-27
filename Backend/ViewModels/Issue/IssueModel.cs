using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DataTypes.Enums;

namespace IssueBase.Issue
{
    using Newtonsoft.Json;

    public class IssueModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public int LocationId { get; set; }
        [JsonProperty(PropertyName = "Id")]
        public int ServerId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public IssueStatus Status { get; set; }

        public IssueSeverity Severity { get; set; }

        public DateTime Created { get; set; }

        public DateTime Edited { get; set; }

        public string CreatedBy { get; set; }


        public bool IssueNeedSync = true; // Issue is saved localy, But not sent to server. Need syncing
        public bool IsNewIssue;
        public bool IssueChanged;
      

        public IssueModel()
        {
            this.ServerId = 0;
            Title = "";
            Description = "";
            Created = DateTime.Now;
            Edited = DateTime.Now;
        }

        public IssueModel(IssueModel issue)
        {
            LocationId = issue.LocationId;
            this.ServerId = issue.ServerId;
            Title = issue.Title;
            Description = issue.Description;
            Longitude = issue.Longitude;
            Latitude = issue.Latitude;
            Status = issue.Status;
            Severity = issue.Severity;
            Created = issue.Created;
            Edited = issue.Edited;
            CreatedBy = issue.CreatedBy;
        }

        public IssueModel CreateUpdatedIssueViewModel()
        {
            IssueModel issue = new IssueModel
            {
                ServerId = this.ServerId,
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


        public IssueSeverity SeverityEx
        {
            get
            {
                return this.Severity;
            }
            set
            {
                Severity = value;
                NotifyPropertyChanged("SeverityImagePath");
                Changed = true;
            }
        }

        public IssueStatus StatusEx
        {
            get
            {
                return this.Status;
            }
            set
            {
                Status = value;
                NotifyPropertyChanged("StatusImagePath");
                Changed = true;
            }
        }

        public string TitleEx
        {
            get
            {
                return this.Title;
            }
            set
            {
                if (this.Title != value)
                {
                    this.Title = value;
                    NotifyPropertyChanged();
                    Changed = true;
                }
            }
        }

        public string CreatedByEx
        {
            get
            {
                return this.CreatedBy;
            }
            set
            {
                if (this.CreatedBy != value)
                {
                    this.CreatedBy = value;
                    NotifyPropertyChanged();
                    Changed = true;
                }
            }
        }

        public string DescriptionEx
        {
            get
            {
                return this.Description;
            }
            set
            {
                if (this.Description != value)
                {
                    this.Description = value;
                    NotifyPropertyChanged();
                    Changed = true;
                }
            }
        }

      

        public DateTime CreatedEx
        {
            get { return this.Created; }
            set
            {
                if (this.Created == value) return;
                this.Created = value;
                NotifyPropertyChanged();
                Changed = true;

            }
        }

        public string SeverityImagePath
        {
            get
            {
                return GetSeverityImage(Severity);
            }
        }
        public string StatusImagePath
        {
            get
            {
                return GetStatusImage(Status);
            }
        }
        public string GetSeverityImage(IssueSeverity severity)
        {
            switch (severity)
            {
                case IssueSeverity.Lowest: return "severity_1.png";
                case IssueSeverity.Low: return "severity_2.png";
                case IssueSeverity.Medium: return "severity_3.png";
                case IssueSeverity.High: return "severity_4.png";
                case IssueSeverity.Highest: return "severity_5.png";
            }

            return "";
        }
        public string GetStatusImage(IssueStatus status)
        {
            switch (status)
            {
                case IssueStatus.Unresolved: return "statusUnresolved.png";
                case IssueStatus.InProgress: return "statusInProgress.png";
                case IssueStatus.Done: return "statusDone.png";
            }

            return "";
        }

        public bool Changed
        {
            get
            {
                return this.IssueChanged;
            }
            set
            {
                IssueChanged = value;
                NotifyPropertyChanged();
            }
        }

        public List<IssueStatusModel> PossibleStatusValues
        {
            get
            {
                List<IssueStatusModel> items = new List<IssueStatusModel>()
            {
              new IssueStatusModel("statusUnresolved.png",IssueStatus.Unresolved),
              new IssueStatusModel("statusInProgress.png",IssueStatus.InProgress),
              new IssueStatusModel("statusDone.png",IssueStatus.Done)
            };

                return items;
            }
        }
        public List<IssueSeverityModel> PossibleSeverityValues
        {
            get
            {
                List<IssueSeverityModel> items = new List<IssueSeverityModel>()
            {
              new IssueSeverityModel("severity_5.png",IssueSeverity.Highest),
              new IssueSeverityModel("severity_4.png",IssueSeverity.High),
              new IssueSeverityModel("severity_3.png",IssueSeverity.Medium),
              new IssueSeverityModel("severity_2.png",IssueSeverity.Low),
              new IssueSeverityModel("severity_1.png",IssueSeverity.Lowest),
            };

                return items;
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}