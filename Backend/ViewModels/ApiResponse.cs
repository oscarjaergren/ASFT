namespace IssueBase
{
    public enum ApiResult
    {
        Success,
        NotUnique,
        UserNotFound,
        LocationNotFound,
        IssueNotFound,
        IssueImageNotFound,
        InvalidIssueSeverity,
        InvalidIssueStatus
    }

    public class ApiResponse
    {
        public ApiResult Result { get; set; }
        public string JsonContent { get; set; }
    }
}