using System.Net;

namespace IssueManagerApiClient
{
    public class IssueManagerClientApplication : IssueManagerClientBase
    {
        public IssueManagerClientApplication(string host, string storedAccessToken = null) :
            base(host, storedAccessToken)
        {
        }

        public string GetTokenForUser(string userName)
        {
            CheckAccessToken();

            HttpWebRequest request = WebRequestHelper.CreateRequest(HttpVerb.Get,
                "api/Account/v1/CreateTokenForUser?userName=" + userName);

            dynamic token = WebRequestHelper.GetDynamicResponse(request);
            return token.ToString();
        }
    }
}