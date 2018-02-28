using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using ASFT.Client;
using ASFT.IServices;
using ASFT.Models;
using ASFT.ViewModels;
using DataTypes.Enums;
using IssueBase.Issue;
using IssueBase.Location;
using IssueManagerApiClient;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Xamarin.Forms;

namespace ASFT.HelperMethods
{
    public class AppIssueClient
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


        public Task<bool> Init()
        {
            return Task.Run(async () =>
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
                return true;
            });
        }

        public FilteringAndSorting GetFilteringAndSorting()
        {
            return Filtering;
        }

        public int GetCurrentLocationId()
        {
            return State.LocationID;
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

        public void LoadState()
        {
        }

        public void SetCurrentLocation(LocationModel location)
        {
            State.LocationID = location.Id;
            State.LocationName = location.Name;
            State.LocationLatitude = location.Latitude;
            State.LocationLongitude = location.Longitude;
            SaveState();
        }

        public GeoLocation GetCurrentGeoLocation()
        {
            return new GeoLocation(State.LocationLatitude, State.LocationLongitude);
        }

        public LoginViewModel GetCurrentLoginModel()
        {
            // DEBUG
            if (State.Username.Length == 0)
                State.Username = "mudemo";
            if (State.Host.Length == 0)
                State.Host = "http://api.asft.se:8080/";

            // VERY DEBUG. - REMOVE 
            const string password = "mudemo";

            return new LoginViewModel
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
            ApiClient.Login(username, password); // exception is throw if failed.

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

        public List<IssueViewModel> GetIssues(int locationId, bool bUseFilter = true)
        {
            var uiIssues = new List<IssueViewModel>();
            var issues = ApiClient.GetAllIssuesAtLocation(locationId);
            foreach (IssueModel item in issues)
            {
                if (bUseFilter)
                    if (Filtering.IncludeItem(item) == false)
                        continue;
                uiIssues.Add(new IssueViewModel(item));
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

        public ObservableCollection<ImageViewModel> GetImages(int issueId)
        {
            var uiIssues = new ObservableCollection<ImageViewModel>();
            var issues = ApiClient.GetImages(issueId);
            foreach (ImageModel item in issues) uiIssues.Add(new ImageViewModel(item));

            return uiIssues;
        }

        public ImageViewModel GetImageInfo(int imageId)
        {
            ImageModel item = ApiClient.GetImageInfo(imageId);
            return new ImageViewModel(item);
        }

        public Tuple<byte[], string> GetImage(int imageId)
        {
            return ApiClient.GetImage(imageId);
        }

        public void AddIssue(IssueModel uiIssue)
        {
            if (uiIssue.LocationId == 0)
                uiIssue.LocationId = State.LocationID;

            NewIssueModel issue = uiIssue.CreateNewIssueModel();
            int issueId = ApiClient.CreateIssue(issue);
            if (issueId > 0)
                uiIssue.Id = issueId;

            if (uiIssue.IsNewIssue)
                uiIssue.IsNewIssue = false;
        }

        public void UpdateIssue(IssueModel uiIssue)
        {
            if (uiIssue.LocationId == 0)
                uiIssue.LocationId = State.LocationID;

            IssueModel issue = uiIssue.CreateUpdatedIssueViewModel();
            ApiClient.UpdateIssue(issue);
        }

        public Task<bool> SaveIssue(IssueModel uiIssue)
        {
            return Task.Run(() =>
            {
                try
                {
                    uiIssue.IssueNeedSync = true;
                    const ConnectionType wifi = ConnectionType.WiFi;
                    var connectionTypes = CrossConnectivity.Current.ConnectionTypes;
                    if (connectionTypes.Contains(wifi))
                    {
                        //Save Issue to cache..
                        UserDialogs.Instance.ShowLoading("Saving...", MaskType.Clear);

                        if (uiIssue.IsNewIssue)
                            App.Client.AddIssue(uiIssue);
                        else
                            App.Client.UpdateIssue(uiIssue);


                        UserDialogs.Instance.HideLoading();

                        uiIssue.IssueNeedSync = false;
                    }
                    else
                    {
                        if (CrossConnectivity.Current.IsConnected)
                            App.Client.AddIssue(uiIssue);
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    LastErrorText = ex.Message;
                    return false;
                }
            });
            // Re-Save Issue to cache
        }


        public void DeleteIssue(int issueId)
        {
            ApiClient.DeleteIssue(issueId);
        }


        public Task<string> TakePhoto()
        {
            try
            {
                ICameraHelper c = DependencyService.Get<ICameraHelper>();
                return c.TakePhoto();
            }
            catch (Exception)
            {
                return Task.Run(() => "");
            }
        }

        public Task<string> PickPhoto()
        {
            try
            {
                ICameraHelper c = DependencyService.Get<ICameraHelper>();
                return c.PickExistingPicture();
            }
            catch (Exception)
            {
                return Task.Run(() => "");
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

        public async Task<bool> PhotoUpload(int issueId, Action<UploadImageEvent, int> onCallback, byte[] bytes,
            string file)
        {
            if (file.Length == 0)
                return false;

            onCallback?.Invoke(UploadImageEvent.ImageCaptured, 0);

            onCallback?.Invoke(UploadImageEvent.ImageUploading, 0);

            int imageId = await UploadPhoto(issueId, bytes, file, false);


            if (onCallback == null) return true;
            if (imageId == 0)
                onCallback(UploadImageEvent.ImageUploadFailed, 0);
            else
                onCallback(UploadImageEvent.ImageUploadSucess, imageId);
            return true;
        }

        public async Task<int> UploadPhoto(int issueId, byte[] bytes, string file, bool bMove)
        {
            string filetype = GetFileType(file);
            int imageId = await UploadImage(issueId, bytes, filetype);
            if (imageId <= 0) return imageId;
            if (bMove)
                MoveFileToImageCache(file, issueId, imageId);
            else
                CopyFileToImageCache(file, issueId, imageId);
            return imageId;
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