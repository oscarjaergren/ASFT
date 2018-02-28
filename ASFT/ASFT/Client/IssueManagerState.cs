namespace ASFT.Client
{
    public class IssueManagerState
    {
        public IssueManagerState()
        {
            Host = "";
            Username = "";
            AccessToken = "";
            LocationName = "";
            LocationID = -1;
            LocationLatitude = 0;
            LocationLongitude = 0;
        }

        public string Host { get; set; }
        public string Username { get; set; }
        public string AccessToken { get; set; }

        public string LocationName { get; set; }
        public double LocationLongitude { get; set; }
        public double LocationLatitude { get; set; }
        public int LocationID { get; set; }
    }
}