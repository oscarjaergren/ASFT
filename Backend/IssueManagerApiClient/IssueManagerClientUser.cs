using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IssueBase;
using IssueBase.Issue;
using IssueBase.Location;
using Newtonsoft.Json;

namespace IssueManagerApiClient
{
    public class IssueManagerClientUser : IssueManagerClientBase
    {
        public IssueManagerClientUser(string host, string storedAccessToken = null) :
            base(host, storedAccessToken)
        {
        }

        #region Locations

        /// <summary>Get all locations, regardless of the current users access rights. </summary>
        public List<LocationModel> GetLocations()
        {
            return GetResource<List<LocationModel>>("api/Location/v1/GetLocations");
        }

        /// <summary>Get all locations to which the current user has access rights. </summary>
        /// <exception cref="UserNotFoundException">The current user wasn't found.</exception>
        public List<LocationModel> GetUsersLocations()
        {
            return GetResource<List<LocationModel>>("api/Location/v1/GetUsersLocations");
        }

        /// <summary>Get a specific location</summary>
        /// <exception cref="LocationNotFoundException">The location doesn't exist.</exception>
        public LocationModel GetLocation(int id)
        {
            return GetResource<LocationModel>("api/Location/v1/GetLocation?locationId=" + id);
        }

        /// <summary>
        ///     Create a new location.
        ///     <para>&#160;</para>
        ///     <para>Returns the id of the created location.</para>
        /// </summary>
        /// <exception cref="NotUniqueException">The location's name isn't unique.</exception>
        public int CreateLocation(NewLocationModel vm)
        {
            return CreateResource("api/Location/v1/CreateLocation", vm);
        }

        /// <summary>Update a location.</summary>
        /// <param name="vm">
        ///     The id cannot be changed and is solely used to identify the location that is to be updated. The
        ///     locations name must be unique.
        /// </param>
        /// <exception cref="LocationNotFoundException">The location doesn't exist.</exception>
        /// <exception cref="NotUniqueException">The location's name isn't unique.</exception>
        public void UpdateLocation(LocationModel vm)
        {
            UpdateResource("api/Location/v1/UpdateLocation", vm);
        }

        /// <summary>Delete a location</summary>
        /// <exception cref="LocationNotFoundException">The location doesn't exist.</exception>
        public void DeleteLocation(int id)
        {
            DeleteResource("api/Location/v1/DeleteLocation?locationId=" + id);
        }

        #endregion Locations

        #region Issues

        /// <exception cref="LocationNotFoundException">The location doesn't exist.</exception>
        /// <exception cref="UserNotFoundException">The current user wasn't found.</exception>
        /// <exception cref="UnauthorizedAccessException">The current user doesn't have access rights to this location.</exception>
        public List<IssueModel> GetAllIssuesAtLocation(int locationId)
        {
            return GetResource<List<IssueModel>>("api/Issue/v1/GetAllIssuesAtLocation?locationId=" + locationId);
        }

        /// <exception cref="LocationNotFoundException">The issues location doesn't exist.</exception>
        /// <exception cref="UserNotFoundException">The current user wasn't found.</exception>
        /// <exception cref="UnauthorizedAccessException">The current user doesn't have access rights to this issue.</exception>
        /// <exception cref="IssueNotFoundException">The issue doesn't exist.</exception>
        public IssueModel GetIssue(int id)
        {
            return GetResource<IssueModel>("api/Issue/v1/GetIssue?issueId=" + id);
        }

        /// <exception cref="LocationNotFoundException">The issues location doesn't exist.</exception>
        /// <exception cref="UserNotFoundException">The current user wasn't found.</exception>
        /// <exception cref="UnauthorizedAccessException">The current user doesn't have access rights to this location.</exception>
        /// <exception cref="InvalidIssueSeverityException">The severity enum value is invalid.</exception>
        /// <exception cref="InvalidIssueStatusException">The status enum value is invalid.</exception>
        public int CreateIssue(IssueModel model)
        {
            return CreateResource("api/Issue/v1/CreateIssue", model);
        }

        /// <summary>Update an issue.</summary>
        /// <param name="issue">The id cannot be changed and is solely used to identify the issue that is to be updated.</param>
        /// <exception cref="LocationNotFoundException">The issues location doesn't exist.</exception>
        /// <exception cref="UserNotFoundException">The current user wasn't found.</exception>
        /// <exception cref="UnauthorizedAccessException">The current user doesn't have access rights to this issue.</exception>
        /// <exception cref="IssueNotFoundException">The issue doesn't exist.</exception>
        /// <exception cref="InvalidIssueSeverityException">The severity enum value is invalid.</exception>
        /// <exception cref="InvalidIssueStatusException">The status enum value is invalid.</exception>
        public void UpdateIssue(IssueModel issue)
        {
            UpdateResource("api/Issue/v1/UpdateIssue", issue);
        }

        /// <exception cref="LocationNotFoundException">The issues location doesn't exist.</exception>
        /// <exception cref="UserNotFoundException">The current user wasn't found.</exception>
        /// <exception cref="UnauthorizedAccessException">The current user doesn't have access rights to this issue.</exception>
        /// <exception cref="IssueNotFoundException">The issue doesn't exist.</exception>
        /// <exception cref="InvalidIssueStatusException">The status enum value is invalid.</exception>
        public void UpdateIssueStatus(IssueStatusModel statusModel)
        {
            UpdateResource("api/Issue/v1/UpdateIssueStatus", statusModel);
        }

        /// <summary>Delete an issue</summary>
        /// <exception cref="LocationNotFoundException">The issues location doesn't exist.</exception>
        /// <exception cref="UserNotFoundException">The current user wasn't found.</exception>
        /// <exception cref="UnauthorizedAccessException">The current user doesn't have access rights to this issue.</exception>
        /// <exception cref="IssueNotFoundException">The issue doesn't exist.</exception>
        public void DeleteIssue(int id)
        {
            DeleteResource("api/Issue/v1/DeleteIssue?issueId=" + id);
        }

        #endregion Issues

        #region IssueImages

        // ImageType = "png" or "jpg"
        // return the unique ImageID - Can be used to fetch image with using GetImage(int imageid)
        public int UploadImage(int id, byte[] imageBytes, string imageType)
        {
            if (imageType != "png" && imageType != "jpg")
                throw new InvalidFileTypeException();

            using (HttpClient client = new HttpClient())
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                client.BaseAddress = new Uri(WebRequestHelper.Host);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", WebRequestHelper.AccessToken);

                ByteArrayContent fileContent1 = new ByteArrayContent(imageBytes);
                fileContent1.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = imageType
                };
                content.Add(fileContent1);

                client.Timeout = new TimeSpan(0, 2, 0);

                HttpResponseMessage response = client.PostAsync("/api/Image/Upload?issueId=" + id, content).Result;

                string json = response.Content.ReadAsStringAsync().Result;


                ApiResponse res = WebRequestHelper.DecodeJsonTyped<ApiResponse>(json);

                ImageModel image = JsonConvert.DeserializeObject<ImageModel>(res.JsonContent);
                return image.ImageIssueId;
            }
        }

        public List<ImageModel> GetImages(int issueId)
        {
            return GetResource<List<ImageModel>>("api/Image/v1/GetImages?issueId=" + issueId);
        }

        public ImageModel GetImageInfo(int imageId)
        {
            return GetResource<ImageModel>("api/Image/v1/GetImageInfo?imageId=" + imageId);
        }

        public Tuple<byte[], string> GetImage(int imageId)
        {
            CheckAccessToken();

            HttpWebRequest request =
                WebRequestHelper.CreateRequest(HttpVerb.Get, "api/Image/v1/GetImage?imageId=" + imageId);

            var result = Task.Run(async () => await WebRequestHelper.GetResponseBytesAsync(request)).Result;


            return result;
        }

        #endregion IssueImages

        #region Generic rest methods (includes parsing and error handling)

        private T GetResource<T>(string action)
        {
            ApiResponse response = PerformRequest(HttpVerb.Get, action);

            T resource = ParseContent<T>(response);
            return resource;
        }

        private int CreateResource<T>(string action, T vm)
        {
            ApiResponse response = PerformRequest(HttpVerb.Post, action, vm);

            ReturnId content = ParseContent<ReturnId>(response);
            return content.Id;
        }

        private void UpdateResource<T>(string action, T vm)
        {
            PerformRequest(HttpVerb.Put, action, vm);
        }

        public void DeleteResource(string action)
        {
            PerformRequest(HttpVerb.Delete, action);
        }


        private ApiResponse PerformRequest(HttpVerb verb, string action, object vm = null)
        {
            CheckAccessToken();

            HttpWebRequest request = WebRequestHelper.CreateRequest(verb, action);
            RequestContent content = vm != null
                ? new RequestContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(vm)), ContentFormat.Json)
                : null;
            ApiResponse response = WebRequestHelper.GetTypedResponse<ApiResponse>(request, content);
            ThrowExceptionIfError(response.Result);

            return response;
        }

        #endregion Generic rest methods
    }
}