using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using DataTypes;
using IssueBase;
using Newtonsoft.Json;

namespace IssueManagerApiClient
{
    public class ReturnId
    {
        public int Id { get; set; }
    }

    public class IssueManagerClientBase
    {
        private const int _apiVersionUsed = 1;

        internal readonly WebRequestHelper WebRequestHelper;

        public IssueManagerClientBase(string host, string storedAccessToken = null)
        {
            WebRequestHelper = new WebRequestHelper(host, storedAccessToken);
        }

        public int ApiVersionUsed
        {
            get { return _apiVersionUsed; }
        }

        public string AccessToken
        {
            get { return WebRequestHelper.AccessToken; }
        }

        public void Login(string userName, string password)
        {
            try
            {
                HttpWebRequest request = WebRequestHelper.CreateRequest(HttpVerb.Post, "Token", false);

                string content = "grant_type=password&username=" + userName + "&password=" + password;
                var contentBytes = Encoding.UTF8.GetBytes(content);


                var loginResult = WebRequestHelper.GetTypedResponse<Dictionary<string, string>>(request,
                    new RequestContent(contentBytes, ContentFormat.XWwwFormUrlencoded));
                WebRequestHelper.AccessToken = loginResult["access_token"];
            }
            catch (UnexpectedHttpErrorException e)
            {
                if (e.HttpStatusCode == HttpStatusCode.BadRequest)
                    throw new InvalidCredentialsException();
                throw;
            }
        }

        public void LogOut()
        {
            WebRequestHelper.AccessToken = null;
        }

        protected bool CheckAccessToken(bool throwIfMissing = true)
        {
            if (WebRequestHelper.AccessToken != null) return true;
            if (throwIfMissing)
                throw new NotLoggedInException();
            return false;

        }

        public string GetUserName()
        {
            if (WebRequestHelper.AccessToken == null)
                throw new NotLoggedInException();

            HttpWebRequest request = WebRequestHelper.CreateRequest(HttpVerb.Get, "api/Account/v1/GetUserName");

            dynamic userNameObj = WebRequestHelper.GetDynamicResponse(request);
            return userNameObj.ToString();
        }

        public bool GetIsVersionSupported()
        {
            return GetIsVersionSupported(ApiVersionUsed);
        }

        public bool GetIsVersionSupported(int versionNr)
        {
            HttpWebRequest request = WebRequestHelper.CreateRequest(HttpVerb.Get,
                "api/Version/IsVersionSupported?versionNr=" + versionNr, false);
            dynamic result = WebRequestHelper.GetDynamicResponse(request);

            if (!bool.TryParse(result.ToString(), out bool isSupported))
                throw new BadResponseFormatException();

            return isSupported;
        }

        internal void ThrowExceptionIfError(ApiResult result)
        {
            switch (result)
            {
                case ApiResult.Success: break;
                case ApiResult.NotUnique: throw new NotUniqueException();
                case ApiResult.UserNotFound: throw new UserNotFoundException();
                case ApiResult.LocationNotFound: throw new LocationNotFoundException();
                case ApiResult.IssueNotFound: throw new IssueNotFoundException();
                case ApiResult.IssueImageNotFound: throw new IssueImageNotFoundException();
                case ApiResult.InvalidIssueSeverity: throw new InvalidIssueSeverityException();
                case ApiResult.InvalidIssueStatus: throw new InvalidIssueStatusException();
                default: throw new Exception("Unknown error.");
            }
        }

        internal dynamic ParseContent(ApiResponse response)
        {
            try
            {
                return JsonConvert.DeserializeObject(response.JsonContent);
            }
            catch (Exception)
            {
                throw new BadResponseFormatException();
            }
        }

        internal T ParseContent<T>(ApiResponse response)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(response.JsonContent);
            }
            catch (Exception)
            {
                throw new BadResponseFormatException();
            }
        }
    }
}