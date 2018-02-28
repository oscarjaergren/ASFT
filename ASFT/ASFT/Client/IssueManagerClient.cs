using System;
using DataTypes;
using DataTypes.Enums;
using IssueBase.Issue;
using Xamarin.Forms;

namespace ASFT.Client
{
    public class FilteringAndSorting : ContentPage
    {
        public FilteringAndSorting()
        {
        }

        public FilteringAndSorting(FilteringAndSorting other)
        {
            SetFrom(other);
        }

        public string SortBy { get; set; }
        public bool SortAscending { get; set; }

        public bool ShowUnresolved { get; set; }
        public bool ShowInProgress { get; set; }
        public bool ShowResolved { get; set; }

        public bool ShowSeverityHighest { get; set; }
        public bool ShowSeverityHigh { get; set; }
        public bool ShowSeverityMedium { get; set; }
        public bool ShowSeverityLow { get; set; }
        public bool ShowSeverityLowest { get; set; }

        public void SetFrom(FilteringAndSorting other)
        {
            SortBy = other.SortBy;
            SortAscending = other.SortAscending;
            ShowUnresolved = other.ShowUnresolved;
            ShowInProgress = other.ShowInProgress;
            ShowResolved = other.ShowResolved;

            ShowSeverityHighest = other.ShowSeverityHighest;
            ShowSeverityHigh = other.ShowSeverityHigh;
            ShowSeverityMedium = other.ShowSeverityMedium;
            ShowSeverityLow = other.ShowSeverityLow;
            ShowSeverityLowest = other.ShowSeverityLowest;
        }

        public static bool operator ==(FilteringAndSorting a, FilteringAndSorting b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if ((object) a == null || (object) b == null) return false;

            if (a.SortBy != b.SortBy)
                return false;
            if (a.SortAscending != b.SortAscending)
                return false;
            if (a.ShowUnresolved != b.ShowUnresolved)
                return false;
            if (a.ShowInProgress != b.ShowInProgress)
                return false;
            if (a.ShowResolved != b.ShowResolved)
                return false;

            if (a.ShowSeverityHighest != b.ShowSeverityHighest)
                return false;
            if (a.ShowSeverityHigh != b.ShowSeverityHigh)
                return false;
            if (a.ShowSeverityMedium != b.ShowSeverityMedium)
                return false;
            if (a.ShowSeverityLow != b.ShowSeverityLow)
                return false;
            if (a.ShowSeverityLowest != b.ShowSeverityLowest)
                return false;

            return true;
        }

        public static bool operator !=(FilteringAndSorting a, FilteringAndSorting b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            throw new Exception("Sorry I don't know what Equals should do for this class");
        }

        public override int GetHashCode()
        {
            throw new Exception("Sorry I don't know what GetHashCode should do for this class");
        }

        public void SetDefault()
        {
            SortBy = "Date";
            SortAscending = true;
            ShowUnresolved = true;
            ShowInProgress = true;
            ShowResolved = true;

            ShowSeverityHighest = true;
            ShowSeverityHigh = true;
            ShowSeverityMedium = true;
            ShowSeverityLow = true;
            ShowSeverityLowest = true;
        }

        public bool IncludeItem(IssueModel item)
        {
            IssueStatus status = item.Status;
            if (status == IssueStatus.Unresolved && ShowUnresolved == false)
                return false;
            if (status == IssueStatus.InProgress && ShowInProgress == false)
                return false;
            if (status == IssueStatus.Done && ShowResolved == false)
                return false;

            IssueSeverity severity = item.Severity;

            if (severity == IssueSeverity.Lowest && ShowSeverityLowest == false)
                return false;
            if (severity == IssueSeverity.Low && ShowSeverityLow == false)
                return false;
            if (severity == IssueSeverity.Medium && ShowSeverityMedium == false)
                return false;
            if (severity == IssueSeverity.High && ShowSeverityHigh == false)
                return false;
            if (severity == IssueSeverity.Highest && ShowSeverityHighest == false)
                return false;

            return true;
        }
    }
}