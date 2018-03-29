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
        public bool FilteringChanged { get; set; }

        public IssueModel Issue { get; set; }

        public bool LoggedIn { get; set; }

        public string StateFilename { get; set; }

        public bool Initilized { get; set; }

        public string LastErrorText { get; set; }

        private const string Defaulthost = "http://api.asft.se:8080/";

        private IssueManagerClientUser apiClient;

        private FilteringAndSorting filtering;

        private IssueManagerState state;

        public AppIssueClient()
        {
            LastErrorText = string.Empty;
            FilteringChanged = false;
            Initilized = false;
            LoggedIn = false;
            apiClient = new IssueManagerClientUser(Defaulthost);
            state = new IssueManagerState();
            filtering = new FilteringAndSorting();
        }

        public async Task<bool> Init()
        {
            await Task.Run(async () =>
             {
                 IFileHelper service = DependencyService.Get<IFileHelper>();

                 string content = await service.Load("Filtering.dat");
                 if (content.Length > 0)
                     this.filtering = JsonConvert.DeserializeObject<FilteringAndSorting>(content);
                 else
                     this.filtering.SetDefault();

                 content = await service.Load("AppState.dat");
                 if (content.Length > 0)
                 {
                     this.state = JsonConvert.DeserializeObject<IssueManagerState>(content);

                     if (this.state.Host.Length == 0)
                         this.state.Host = Defaulthost;

                     this.apiClient = new IssueManagerClientUser(this.state.Host, this.state.AccessToken);
                     LoggedIn = this.state.AccessToken.Length > 0;
                 }

                 Initilized = true;
                 SaveState();
                 return true;
             });
            return false;
        }


        public async Task<bool> ShowSelectLocation()
        {
            var locations = GetLocatonsOnline();
            if (locations != null)
                return await OnShowSelectLocationSheet(locations);

            return false;
        }


        public FilteringAndSorting GetFilteringAndSorting()
        {
            return this.filtering;
        }

        public int GetCurrentLocationId()
        {
            return this.state.LocationId;
        }

        public string GetCurrentLocationName()
        {
            return this.state.LocationName;
        }

        public string GetCurrentUsername()
        {
            return this.state.Username;
        }

        public void SaveState()
        {
            try
            {
                string content = JsonConvert.SerializeObject(this.state);
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
                string content = JsonConvert.SerializeObject(this.filtering);
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
            this.state.LocationId = location.Id;
            this.state.LocationName = location.Name;
            state.LocationLatitude = location.Latitude;
            this.state.LocationLongitude = location.Longitude;
            SaveState();
        }

        public GeoLocation GetCurrentGeoLocation()
        {
            return new GeoLocation(this.state.LocationLatitude, this.state.LocationLongitude);
        }

        public LoginModel GetCurrentLoginModel()
        {
            // DEBUG
            if (this.state.Username.Length == 0)
                this.state.Username = "mudemo";
            if (this.state.Host.Length == 0)
                this.state.Host = "http://api.asft.se:8080/";

            // VERY DEBUG. - REMOVE 
            const string Password = "mudemo";

            return new LoginModel()
            {
                // default debug account
                Host = this.state.Host,
                Username = this.state.Username,
                Password = Password
            };
        }

        public void Logout()
        {
            LoggedIn = false;
            this.apiClient = new IssueManagerClientUser(this.state.Host);
            this.state.AccessToken = string.Empty;
            SaveState();
        }

        public bool Login(string host, string username, string password)
        {
            LoggedIn = false;

            this.apiClient = new IssueManagerClientUser(host);
            this.apiClient.Login(username, password);

            // Store the userinformation for next time
            this.state.Host = host;
            this.state.Username = username;
            this.state.AccessToken = this.apiClient.AccessToken;
            SaveState();
            LoggedIn = true;

            return true;
        }

        public List<LocationModel> GetLocations()
        {
            return this.apiClient.GetLocations();
        }

        public List<IssueModel> GetIssues(int locationId, bool useFilter = true)
        {
            var uiIssues = new List<IssueModel>();
            var issues = this.apiClient.GetAllIssuesAtLocation(locationId);
            foreach (IssueModel item in issues)
            {
                if (useFilter)
                    if (this.filtering.IncludeItem(item) == false)
                        continue;
                uiIssues.Add(new IssueModel(item));
            }

            if (!useFilter) return uiIssues;

            switch (this.filtering.SortBy)
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

            if (this.filtering.SortAscending == false)
                uiIssues.Reverse();

            return uiIssues;
        }

        public ObservableCollection<ImageModel> GetImageInfo(int issueId)
        {
            var uiIssues = new ObservableCollection<ImageModel>();
            var issues = this.apiClient.GetImages(issueId);
            foreach (ImageModel item in issues) uiIssues.Add(new ImageModel(item));

            return uiIssues;
        }

        public ImageModel GetImages(int imageId)
        {
            ImageModel item = this.apiClient.GetImageInfo(imageId);
            return new ImageModel(item);
        }

        public Tuple<byte[], string> GetImage(int imageId)
        {
            return this.apiClient.GetImage(imageId);
        }

        public async Task AddIssue(IssueModel issue)
        {
            if (issue.LocationId == 0 || issue.LocationId == -1) await App.Client.ShowSelectLocation();
            issue.LocationId = GetCurrentLocationId();

            int issueServerId = this.apiClient.CreateIssue(issue);

            if (issueServerId > 0) issue.ServerId = issueServerId;

            if (issue.IsNewIssue) issue.IsNewIssue = false;
        }

        public void UpdateIssue(IssueModel uiIssue)
        {
            if (uiIssue.LocationId == 0)
                uiIssue.LocationId = this.state.LocationId;

            IssueModel issue = uiIssue.CreateUpdatedIssueViewModel();
            this.apiClient.UpdateIssue(issue);
        }

        public Task<bool> SaveIssue(IssueModel issue)
        {
            return Task.Run(async () =>
            {
                try
                {
                    issue.IssueNeedSync = true;
                    const ConnectionType Wifi = ConnectionType.WiFi;
                    var connectionTypes = CrossConnectivity.Current.ConnectionTypes;
                    if (connectionTypes.Contains(Wifi))
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
            this.apiClient.DeleteIssue(issueId);
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

        public async Task<int> UploadPhoto(int issueId, byte[] bytes, string file, bool move)
        {
            string filetype = GetFileType(file);
            int imageResult = await UploadImage(issueId, bytes, filetype);
            if (imageResult <= 0) return imageResult;
            if (move)
                MoveFileToImageCache(file, issueId, imageResult);
            else
                CopyFileToImageCache(file, issueId, imageResult);
            return imageResult;
        }

        public Task<int> UploadImage(int issueId, byte[] bytes, string imagetype)
        {
            return Task.Run(() =>
            {
                try
                {
                    return bytes.Length > 0 ? this.apiClient.UploadImage(issueId, bytes, imagetype) : 0;
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

        public Task<string> GetThumbnail(int imgId, int issueId, int thumbSize, bool download)
        {
            return Task.Run(() =>
            {
                try
                {
                    string thumbPath = GetThumbnailFilePath(imgId, issueId, thumbSize);
                    if (this.FileExists(thumbPath)) return thumbPath;
                    string orgImagePath = GetImageFilePath(imgId, issueId);
                    if (FileExists(orgImagePath) == false)
                    {
                        if (download == false)
                            return string.Empty;

                        var imageData = App.Client.GetImage(imgId);

                        SaveImageDataToFile(orgImagePath, imageData.Item1);
                    }

                    CreateThumbnail(orgImagePath, thumbPath, thumbSize);

                    return thumbPath;
                }
                catch (Exception)
                {
                    return string.Empty;
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

        private static string GetFileType(string file)
        {
            string filetype = "jpg";
            int fileExtPos = file.LastIndexOf('.');
            if (fileExtPos <= 0) return filetype;
            filetype = file.Substring(fileExtPos + 1);
            filetype = filetype.ToLower();
            return filetype;
        }

        private static void CreateThumbnail(string originalImageFile, string thumbnailFile, int maxSizeIn)
        {
            ImageSize maxSize = new ImageSize(maxSizeIn, maxSizeIn);
            ThumbnailCreator.CreateThumbnail(originalImageFile, thumbnailFile, maxSize);
        }

        private List<LocationModel> GetLocatonsOnline()
        {
            List<LocationModel> locations = null;
            try
            {
                locations = GetLocations();
            }
            catch (ServerNotFoundException)
            {
                CoreMethods.DisplayAlert("Failed", "Failed to connect to server", "Continue");
            }
            catch (NotLoggedInException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                CoreMethods.DisplayAlert("Failed", "Unknown error", "Quit");
                throw;
            }

            return locations;
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
                CancellationToken s = new CancellationToken();
                string res = await UserDialogs.Instance.ActionSheetAsync("Pick Location", "Cancel", string.Empty, s, buttons);
                if (res == "Cancel")
                    return false;

                string locationName = string.Empty;
                int id = Convert.ToInt32(res.Substring(0, 2));
                int pos = res.IndexOf('-');
                if (pos > 0)
                    locationName = res.Substring(pos + 1);

                this.state.LocationName = locationName.Trim();

                if (id > 0)
                {
                    foreach (LocationModel loc in locations)
                    {
                        if (loc.Id == id)
                        {
                            state.LocationId = id;
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
    }
}