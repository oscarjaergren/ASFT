using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using ASFT.Client;
using ASFT.IServices;
using ASFT.Models;
using ASFT.PageModels;
using DataTypes.Enums;
using FreshMvvm;
using IssueBase.Issue;
using IssueBase.Location;
using IssueManagerApiClient;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Xamarin.Forms;

namespace ASFT.HelperMethods
{
    public class AppIssueClient : FreshBasePageModel
    {
        private const string Defaulthost = "http://api.asft.se:8080/";

        protected IssueManagerClientUser ApiClient;
        protected FilteringAndSorting Filtering;
        protected IssueManagerState State;

        public AppIssueClient()
        {
            LastErrorText = "";
            FilteringChanged = false;
            Initilized = false;
            LoggedIn = false;
            ApiClient = new IssueManagerClientUser(Defaulthost);
            State = new IssueManagerState();
            Filtering = new FilteringAndSorting();
        }

        public bool FilteringChanged { get; set; }
        public bool LoggedIn { get; set; }
        public string StateFilename { get; set; }
        public bool Initilized { get; set; }
        public string LastErrorText { get; set; }


        public async Task<bool> Init()
        {
            await Task.Run(async () =>
             {
                 IFileHelper service = DependencyService.Get<IFileHelper>();

                 string content = await service.Load("Filtering.dat");
                 if (content.Length > 0)
                     Filtering = JsonConvert.DeserializeObject<FilteringAndSorting>(content);
                 else
                     Filtering.SetDefault();

                 content = await service.Load("AppState.dat");
                 if (content.Length > 0)
                 {
                     State = JsonConvert.DeserializeObject<IssueManagerState>(content);

                     if (State.Host.Length == 0)
                         State.Host = Defaulthost;

                     ApiClient = new IssueManagerClientUser(State.Host, State.AccessToken);
                     LoggedIn = State.AccessToken.Length > 0;
                 }

                 Initilized = true;
                 SaveState();
                 return true;
             });
            return false;
        }

        private async Task<bool> OnShowSelectLocationSheet(List<LocationModel> locations)
        {
            try
            {
                var buttons = new string[locations.Count];
                for (int n = 0; n < locations.Count; ++n)
                {
                    buttons[n] = locations[n].Id + " - " + locations[n].Name;
                }
                var s = new CancellationToken();
                string res = await UserDialogs.Instance.ActionSheetAsync("Pick Location", "Cancel", "", s, buttons);
                if (res == "Cancel")
                    return false;

                string locationName = "";
                int id = Convert.ToInt32(res.Substring(0, 2));
                int pos = res.IndexOf('-');
                if (pos > 0)
                    locationName = res.Substring(pos + 1);

                State.LocationName = locationName.Trim();

                if (id > 0)
                {
                    foreach (LocationModel loc in locations)
                    {
                        if (loc.Id == id)
                        {
                            State.LocationId = id;
                            App.Client.SetCurrentLocation(loc);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                return false;
            }
        }
        public async Task<bool> ShowSelectLocation()
        {
            var locations = GetLocatonsOnline();
            if (locations != null)
                return await OnShowSelectLocationSheet(locations);

            return false;
        }
        private List<LocationModel> GetLocatonsOnline()
        {
            List<LocationModel> locations = null;
            try
            {
                locations = GetLocations();
            }
            catch (IssueManagerApiClient.ServerNotFoundException)
            {
                CoreMethods.DisplayAlert("Failed", "Failed to connect to server", "Continue");
            }
            catch (IssueManagerApiClient.NotLoggedInException /*ex*/)
            {
                MessagingCenter.Subscribe<LoginPageModel>(this, "OnLoggedIn", (sender) =>
                {
                    MessagingCenter.Unsubscribe<LoginPageModel>(this, "OnLoggedIn");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                CoreMethods.DisplayAlert("Failed", "Unknown error", "Quit");
                throw;
            }
            return locations;

        }




        public FilteringAndSorting GetFilteringAndSorting()
        {
            return Filtering;
        }

        public int GetCurrentLocationId()
        {
            return State.LocationId;
        }

        public string GetCurrentLocationName()
        {
            return State.LocationName;
        }

        public string GetCurrentUsername()
        {
            return State.Username;
        }

        public void SaveState()
        {
            try
            {
                string content = JsonConvert.SerializeObject(State);
                IFileHelper save = DependencyService.Get<IFileHelper>();
                save.Save("AppState.dat", content);
            }
            catch (Exception)
            {
                // Show error
            }
        }

        public void SaveFiltering()
        {
            try
            {
                string content = JsonConvert.SerializeObject(Filtering);
                IFileHelper save = DependencyService.Get<IFileHelper>();
                save.Save("Filtering.dat", content);
            }
            catch (Exception)
            {
                // Show error
            }
        }

        public void SetCurrentLocation(LocationModel location)
        {
            State.LocationId = location.Id;
            State.LocationName = location.Name;
            State.LocationLatitude = location.Latitude;
            State.LocationLongitude = location.Longitude;
            SaveState();
        }

        public GeoLocation GetCurrentGeoLocation()
        {
            return new GeoLocation(State.LocationLatitude, State.LocationLongitude);
        }

        public LoginModel GetCurrentLoginModel()
        {
            // DEBUG
            if (State.Username.Length == 0)
                State.Username = "mudemo";
            if (State.Host.Length == 0)
                State.Host = "http://api.asft.se:8080/";

            // VERY DEBUG. - REMOVE 
            const string password = "mudemo";

            return new LoginModel()
            {
                // default debug account
                Host = State.Host,
                Username = State.Username,
                Password = password
            };
        }

        public void Logout()
        {
            LoggedIn = false;
            ApiClient = new IssueManagerClientUser(State.Host);
            State.AccessToken = "";
            SaveState();
        }

        public bool Login(string host, string username, string password)
        {
            LoggedIn = false;

            ApiClient = new IssueManagerClientUser(host);
            ApiClient.Login(username, password);

            //if successsfull no excpetion was throw so we can store new state variables
            State.Host = host;
            State.Username = username;
            State.AccessToken = ApiClient.AccessToken;
            SaveState();
            LoggedIn = true;

            return true;
        }

        public List<LocationModel> GetLocations()
        {
            return ApiClient.GetLocations();
        }

        public List<IssueModel> GetIssues(int locationId, bool bUseFilter = true)
        {
            var uiIssues = new List<IssueModel>();
            var issues = ApiClient.GetAllIssuesAtLocation(locationId);
            foreach (IssueModel item in issues)
            {
                if (bUseFilter)
                    if (Filtering.IncludeItem(item) == false)
                        continue;
                uiIssues.Add(new IssueModel(item));
            }

            if (!bUseFilter) return uiIssues;
            // sort
            switch (Filtering.SortBy)
            {
                case "Date":
                    uiIssues = uiIssues.OrderBy(o => o.Created).ToList();
                    break;
                case "Title":
                    uiIssues = uiIssues.OrderBy(o => o.Title).ToList();
                    break;
                case "Status":
                    uiIssues = uiIssues.OrderBy(o => o.Status).ToList();
                    break;
                case "Severity":
                    uiIssues = uiIssues.OrderBy(o => o.Severity).ToList();
                    break;
            }

            if (Filtering.SortAscending == false)
                uiIssues.Reverse();

            return uiIssues;
        }

        public ObservableCollection<ImageModel> GetImages(int issueId)
        {
            var uiIssues = new ObservableCollection<ImageModel>();
            var issues = ApiClient.GetImages(issueId);
            foreach (ImageModel item in issues) uiIssues.Add(new ImageModel(item));

            return uiIssues;
        }

        public ImageModel GetImageInfo(int imageId)
        {
            ImageModel item = ApiClient.GetImageInfo(imageId);
            return new ImageModel(item);
        }

        public Tuple<byte[], string> GetImage(int imageId)
        {
            return ApiClient.GetImage(imageId);
        }

        public async Task AddIssue(IssueModel issue)
        {
            if (issue.LocationId == 0 || issue.LocationId == -1) await App.Client.ShowSelectLocation();
            issue.LocationId = GetCurrentLocationId();

            int issueServerId = ApiClient.CreateIssue(issue);

            if (issueServerId > 0) issue.ServerId = issueServerId;

            if (issue.IsNewIssue) issue.IsNewIssue = false;
        }

        public void UpdateIssue(IssueModel uiIssue)
        {
            if (uiIssue.LocationId == 0)
                uiIssue.LocationId = State.LocationId;

            IssueModel issue = uiIssue.CreateUpdatedIssueViewModel();
            ApiClient.UpdateIssue(issue);
        }

        public Task<bool> SaveIssue(IssueModel issue)
        {
            return Task.Run(async () =>
            {
                try
                {
                    issue.IssueNeedSync = true;
                    const ConnectionType wifi = ConnectionType.WiFi;
                    var connectionTypes = CrossConnectivity.Current.ConnectionTypes;
                    if (connectionTypes.Contains(wifi))
                    {
                        UserDialogs.Instance.ShowLoading("Saving...", MaskType.Clear);

                        if (issue.IsNewIssue)
                            await App.Client.AddIssue(issue);
                        else
                            App.Client.UpdateIssue(issue);


                        UserDialogs.Instance.HideLoading();

                        issue.IssueNeedSync = false;

                    }
                    else
                    {
                        if (CrossConnectivity.Current.IsConnected)
                            await App.Client.AddIssue(issue);
                        return false;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LastErrorText = ex.Message;
                    Debug.WriteLine(ex + LastErrorText);
                    return false;
                }
            });
        }
        public void DeleteIssue(int issueId)
        {
            ApiClient.DeleteIssue(issueId);
        }

        private static string GetFileType(string file)
        {
            string filetype = "jpg";
            int fileExtPos = file.LastIndexOf('.');
            if (fileExtPos <= 0) return filetype;
            filetype = file.Substring(fileExtPos + 1);
            filetype = filetype.ToLower();
            return filetype;
        }

        public async Task<bool> PhotoUpload(int issueId, Action<UploadImageEvent, int> onCallback, byte[] bytes, string file)
        {
            if (file.Length == 0)
                return false;

            onCallback?.Invoke(UploadImageEvent.ImageCaptured, 0);

            onCallback?.Invoke(UploadImageEvent.ImageUploading, 0);

            int imageResult = await UploadPhoto(issueId, bytes, file, false);


            if (onCallback == null) return true;
            if (imageResult == 0)
                onCallback(UploadImageEvent.ImageUploadFailed, 0);
            else
                onCallback(UploadImageEvent.ImageUploadSucess, imageResult);
            return true;
        }

        public async Task<int> UploadPhoto(int issueId, byte[] bytes, string file, bool bMove)
        {
            string filetype = GetFileType(file);
            int imageResult = await UploadImage(issueId, bytes, filetype);
            if (imageResult <= 0) return imageResult;
            if (bMove)
                MoveFileToImageCache(file, issueId, imageResult);
            else
                CopyFileToImageCache(file, issueId, imageResult);
            return imageResult;
        }

        // Move to background thread
        public Task<int> UploadImage(int issueId, byte[] bytes, string imagetype)
        {
            return Task.Run(() =>
            {
                try
                {
                    return bytes.Length > 0 ? ApiClient.UploadImage(issueId, bytes, imagetype) : 0;
                }
                catch (Exception)
                {
                    return 0;
                }
            });
        }
        public void MoveFileToImageCache(string filename, int issueId, int imageId)
        {
            IFileHelper fileHelper = DependencyService.Get<IFileHelper>();

            string targetFile = GetImageFilePath(imageId, issueId);

            if (!fileHelper.MakeSureDirectoryExists(targetFile, true)) return;
            if (FileExists(targetFile)) DeleteFile(targetFile);

            fileHelper.MoveFile(filename, targetFile);
        }

        public void CopyFileToImageCache(string filename, int issueId, int imageId)
        {
            IFileHelper fileHelper = DependencyService.Get<IFileHelper>();

            string targetFile = GetImageFilePath(imageId, issueId);

            if (!fileHelper.MakeSureDirectoryExists(targetFile, true)) return;
            if (FileExists(targetFile)) DeleteFile(targetFile);

            fileHelper.CopyFile(filename, targetFile);
        }

        public void RunInBackground(Action action)
        {
            IThreadHelper threadHelper = DependencyService.Get<IThreadHelper>();
            threadHelper.RunInBackground(action);
        }


        public bool ClearImageCacheForIssue(int issueId)
        {
            IFileHelper fileHelper = DependencyService.Get<IFileHelper>();
            string path = DependencyService.Get<IDirectoryService>().GetImagesCacheDirectory();
            path = Path.Combine(path, issueId.ToString());
            return fileHelper.DeleteFolder(path, true);
        }

        // "<CacheDir>\IssueID\<ImgID>.jpg"
        public string GetImageFilePath(int imgId, int issueId)
        {
            string path = DependencyService.Get<IDirectoryService>().GetImagesCacheDirectory();
            path = Path.Combine(path, issueId.ToString());
            path = Path.Combine(path, imgId.ToString());
            path += ".jpg";

            return path;
        }

        public string GetThumbnailFilePath(int imgId, int issueId, int thumbSize)
        {
            string path = DependencyService.Get<IDirectoryService>().GetImagesCacheDirectory();
            path = Path.Combine(path, issueId.ToString());
            path = Path.Combine(path, imgId.ToString());
            path += "_thumb" + thumbSize;
            path += ".jpg";
            return path;
        }

        private static void CreateThumbnail(string originalImageFile, string thumbnailFile, int maxSizeIn)
        {
            ImageSize maxSize = new ImageSize(maxSizeIn, maxSizeIn);
            ThumbnailCreator.CreateThumbnail(originalImageFile, thumbnailFile, maxSize);
        }

        public Task<string> GetThumbnail(int imgId, int issueId, int thumbSize, bool bDownload)
        {
            return Task.Run(() =>
            {
                try
                {
                    string thumbPath = GetThumbnailFilePath(imgId, issueId, thumbSize);
                    if (FileExists(thumbPath) != false) return thumbPath;
                    string orgImagePath = GetImageFilePath(imgId, issueId);
                    if (FileExists(orgImagePath) == false)
                    {
                        if (bDownload == false)
                            return "";

                        var imageData = App.Client.GetImage(imgId);

                        SaveImageDataToFile(orgImagePath, imageData.Item1);
                    }

                    CreateThumbnail(orgImagePath, thumbPath, thumbSize);

                    return thumbPath;
                }
                catch (Exception)
                {
                    return "";
                }
            });
        }

        public bool FileExists(string imgFile)
        {
            return DependencyService.Get<IFileHelper>().Exists(imgFile);
        }

        public void DeleteFile(string filepath)
        {
            DependencyService.Get<IFileHelper>().DeleteFile(filepath);
        }

        public bool SaveImageDataToFile(string filename, byte[] data)
        {
            IFileHelper fileHelper = DependencyService.Get<IFileHelper>();

            if (fileHelper.MakeSureDirectoryExists(filename, true))
            {
                fileHelper.WriteFile(filename, data);
                return true;
            }

            return false;
        }

        public bool SaveImageDataToCache(byte[] data, int imageId, int issueId, string dataType)
        {
            try
            {
                IFileHelper fileHelper = DependencyService.Get<IFileHelper>();

                string filename = GetImageFilePath(imageId, issueId);

                if (fileHelper.MakeSureDirectoryExists(filename, true)) fileHelper.WriteFile(filename, data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}