namespace ASFT.PageModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Acr.UserDialogs;
    using ASFT.IServices;

    using DataTypes.Enums;
    using FreshMvvm;
    using IssueBase.Issue;
    using Plugin.Geolocator;
    using Plugin.Geolocator.Abstractions;
    using Plugin.Media;
    using Plugin.Media.Abstractions;

    using TK.CustomMap;
    using TK.CustomMap.Api;
    using TK.CustomMap.Api.Google;
    using TK.CustomMap.Api.OSM;
    using TK.CustomMap.Interfaces;
    using TK.CustomMap.Overlays;

    using Xamarin.Forms;

    using Position = Plugin.Geolocator.Abstractions.Position;

    [SuppressMessage("ReSharper", "InvertIf")]
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class IssuePageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Model

        private IGeolocator geolocator;

        private string locationText;

        private ICommand onGoToListCommand;
        private ICommand submitCommand;
        private ICommand onStatusClickedCommand;

        #region Properties

        public List<IssueSeverityModel> SeverityValues { get; set; }

        public List<IssueStatusModel> StatusValues { get; set; }

        public bool IsBusy { get; set; }

        public string StatusText { get; set; }

        public string ImageText { get; set; }

        public IssueSeverity SeverityEx
        {
            get { return App.Client.Issue.Severity; }
            set
            {
                App.Client.Issue.Severity = value;
                this.RaisePropertyChanged(nameof(SeverityEx));
                App.Client.Issue.Changed = true;
            }
        }

        public IssueStatus StatusEx
        {
            get { return App.Client.Issue.Status; }
            set
            {
                App.Client.Issue.Status = value;
                this.RaisePropertyChanged(nameof(StatusEx));
                App.Client.Issue.Changed = true;
            }
        }

        public string LocationText
        {
            get { return App.Client.GetCurrentLocationName(); }
            set
            {
                locationText = value;
                this.RaisePropertyChanged(nameof(LocationText));
            }
        }

        public string TitleEx
        {
            get { return App.Client.Issue.Title; }
            set
            {
                if (App.Client.Issue.Title == value) return;
                App.Client.Issue.Title = value;
                this.RaisePropertyChanged(nameof(TitleEx));
                App.Client.Issue.Changed = true;
            }
        }

        public string CreatedByEx
        {
            get { return App.Client.Issue.CreatedBy; }
            set
            {
                if (App.Client.Issue.CreatedBy == value) return;
                App.Client.Issue.CreatedBy = value;
                this.RaisePropertyChanged(nameof(CreatedByEx));
                App.Client.Issue.Changed = true;
            }
        }

        public DateTime CreatedEx
        {
            get { return App.Client.Issue.Created; }
            set
            {
                if (App.Client.Issue.Created == value) return;
                App.Client.Issue.Created = value;
                this.RaisePropertyChanged(nameof(CreatedEx));
                App.Client.Issue.Changed = true;
            }
        }

        public string DescriptionEx
        {
            get { return App.Client.Issue.Description; }
            set
            {
                if (App.Client.Issue.Description == value) return;
                App.Client.Issue.Description = value;
                this.RaisePropertyChanged(nameof(DescriptionEx));
                App.Client.Issue.Changed = true;
            }
        }
        #region Opacity Properties

        public double Severity5Opacity { get; set; }

        public double Severity4Opacity { get; set; }

        public double Severity3Opacity { get; set; }

        public double Severity2Opacity { get; set; }

        public double Severity1Opacity { get; set; }

        public double StatusUnresolvedOpacity { get; set; }

        public double StatusInProgressOpacity { get; set; }

        public double StatusDoneOpacity { get; set; }

        #endregion
        #endregion


        #region Command


        public ICommand OnStatusClickedCommand
        {
            get
            {
                return onStatusClickedCommand ?? (onStatusClickedCommand = new Command<object>(OnStatusTapped));
            }
        }

        public ICommand OnGoToListCommand
        {
            get
            {
                return onGoToListCommand ?? (onGoToListCommand = new Command(OnGoToList));
            }
        }

        public ICommand SubmitCommand
        {
            get
            {
                return submitCommand ?? (submitCommand = new Command(OnSubmit));
            }
        }

        #endregion

        #endregion

        #region Onstart

        #region Initilzation
        public IssuePageModel()
        {
            geolocator = CrossGeolocator.Current;


            if (App.Client.Issue == null)
            {
                App.Client.Issue = CreateIssueModel();

                TitleEx = "New Event";
                SeverityEx = IssueSeverity.Medium;
                StatusEx = IssueStatus.InProgress;
                GetLocation();
            }
            StatusValues = App.Client.Issue.PossibleStatusValues;
            SeverityValues = App.Client.Issue.PossibleSeverityValues;
            StatusChecker();
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            if (initData is IssueModel issue)
            {
                App.Client.Issue = issue;
                if (App.Client.Issue.ServerId != 0) this.GetImagesId(issue.ServerId);
                StatusChecker();
                if (!App.Client.Issue.IsNewIssue)
                {
                    MapCenter = new TK.CustomMap.Position(App.Client.Issue.Longitude, App.Client.Issue.Latitude);
                    MapRegion = MapSpan.FromCenterAndRadius(MapCenter, Distance.FromKilometers(2));
                    TK.CustomMap.Position x = new TK.CustomMap.Position(App.Client.Issue.Longitude, App.Client.Issue.Latitude);
                    MapRegion = MapSpan.FromCenterAndRadius(
                        new TK.CustomMap.Position(x.Latitude, x.Longitude),
                        Distance.FromKilometers(2));
                    AddPin(x);

                }
                UserDialogs.Instance.HideLoading();
            }
        }

        public IssueModel CreateIssueModel()
        {
            return new IssueModel
            {
                LocationId = 0,
                ServerId = 0,
                Title = string.Empty,
                Description = string.Empty,
                Longitude = 0,
                Latitude = 0,
                CreatedBy = string.Empty,
                IsNewIssue = true,
                Created = DateTime.Now,
                Edited = DateTime.Now
            };
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            if (App.Client.Initilized == false) await App.Client.Init();

            if (App.Client.LoggedIn != true)
            {
                await ShowLoginPage();
            }

            await GetLocationName();
            CreatedByEx = App.Client.GetCurrentUsername();
        }


        #endregion



        private async Task<bool> GetLocationName()
        {
            int locationId = App.Client.GetCurrentLocationId();
            if (App.Client.LoggedIn == false) return false;
                
            if (locationId == -1)
            {
                await App.Client.ShowSelectLocation();
                return true;
            }
            return false;
        }



        private async Task ShowLoginPage()
        {
            await CoreMethods.PushPageModel<LoginPageModel>();
        }

        private async void OnGoToList()
        {
            int locationId = App.Client.GetCurrentLocationId();
            await CoreMethods.PushPageModel<IssueListPageModel>(locationId);
        }

        #endregion

        private void StatusChecker()
        {
            switch (App.Client.Issue.Status)
            {
                case IssueStatus.Unresolved:
                    StatusUnresolvedOpacity = 1;
                    StatusInProgressOpacity = 0.5;
                    StatusDoneOpacity = 0.5;
                    StatusText = "Unresolved";
                    break;
                case IssueStatus.InProgress:
                    StatusUnresolvedOpacity = 0.5;
                    StatusInProgressOpacity = 1;
                    StatusDoneOpacity = 0.5;
                    StatusText = "In Progress";
                    break;
                case IssueStatus.Done:
                    StatusUnresolvedOpacity = 0.5;
                    StatusInProgressOpacity = 0.5;
                    StatusDoneOpacity = 1;
                    StatusText = "Done";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (App.Client.Issue.Severity)
            {
                case IssueSeverity.Lowest:
                    Severity1Opacity = 1;
                    Severity2Opacity = 0.5;
                    Severity3Opacity = 0.5;
                    Severity4Opacity = 0.5;
                    Severity5Opacity = 0.5;
                    break;
                case IssueSeverity.Low:
                    Severity1Opacity = 0.5;
                    Severity2Opacity = 1;
                    Severity3Opacity = 0.5;
                    Severity4Opacity = 0.5;
                    Severity5Opacity = 0.5;
                    break;
                case IssueSeverity.Medium:
                    Severity1Opacity = 0.5;
                    Severity2Opacity = 0.5;
                    Severity3Opacity = 1;
                    Severity4Opacity = 0.5;
                    Severity5Opacity = 0.5;
                    break;
                case IssueSeverity.High:
                    Severity1Opacity = 0.5;
                    Severity2Opacity = 0.5;
                    Severity3Opacity = 0.5;
                    Severity4Opacity = 1;
                    Severity5Opacity = 0.5;
                    break;
                case IssueSeverity.Highest:
                    Severity1Opacity = 0.5;
                    Severity2Opacity = 0.5;
                    Severity3Opacity = 0.5;
                    Severity4Opacity = 0.5;
                    Severity5Opacity = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnStatusTapped(object fileName)
        {
            foreach (IssueStatusModel item in StatusValues)
            {
                if (item.Name == (string)fileName)
                {
                    App.Client.Issue.Status = item.Status;
                    StatusChecker();
                    return;
                }
            }

            foreach (IssueSeverityModel item in SeverityValues)
            {
                if (item.Name == (string)fileName)
                {
                    App.Client.Issue.Severity = item.Severity;
                    StatusChecker();
                    return;
                }
            }
        }

        #region Save

        private async void OnSubmit()
        {
            if (IsBusy) return;
            IsBusy = true;

            bool saved = await SaveChanges();

            if (saved)
            {
                UserDialogs.Instance.Toast("Issue has been uploaded");
                var imagesinCollection = Images;

                foreach (ImageModel image in imagesinCollection)
                {
                    UserDialogs.Instance.Toast("Uploading" + image.ImageId);
                    bool imageUploadSuccess = await App.Client.PhotoUpload(
                                                  App.Client.Issue.ServerId,
                                                  OnCallbackUploadImage,
                                                  image.OrgImage,
                                                  image.Image.FileName);
                    if (imageUploadSuccess)
                    {
                        UserDialogs.Instance.Toast("Images succesfully uploaded");
                    }
                    else
                    {
                        UserDialogs.Instance.Toast("Images failed to upload");
                    }
                }
            }
            else
            {
                UserDialogs.Instance.Toast("Issue has been failed");
                UserDialogs.Instance.Alert("Save Failed", "Save failed", "OK");
            }
            IsBusy = false;
        }

        private async Task<bool> SaveChanges()
        {
            try
            {
                await Task.Run(async () => { await App.Client.SaveIssue(App.Client.Issue); });
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Images

        private void OnCallbackUploadImage(UploadImageEvent eventId, int imageId)
        {
            switch (eventId)
            {
                case UploadImageEvent.ImageUploading:
                    Device.BeginInvokeOnMainThread(() => { ImageText = "Uploading Image..."; });
                    break;
                case UploadImageEvent.ImageUploadFailed:
                    Device.BeginInvokeOnMainThread(() => { ImageText = "Failed to upload image"; });
                    break;
                case UploadImageEvent.ImageUploadSucess:
                    ImageModel newimage = App.Client.GetImages(imageId);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ImageText = "Image Uploaded successful";
                        Images.Add(newimage);
                        //App.Client.RunInBackground(DownloadImages);
                    });
                    break;
                case UploadImageEvent.ImageCaptured:
                    break;
            }
        }

        private void AddTheImages(int imageIssueId)
        {
            var imageData = App.Client.GetImage(imageIssueId);

            byte[] imageAsBytes = imageData.Item1;

            if (imageAsBytes.Length > 0)
            {
                IImageResizer resizer = DependencyService.Get<IImageResizer>();
                imageAsBytes = resizer.ResizeImage(imageAsBytes, 1080, 1080);

                ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
                Images.Add(new ImageModel { Source = imageSource, OrgImage = imageAsBytes });

                // Debug to check if images are valid when being download. They are 
                string base64String = Convert.ToBase64String(imageAsBytes);
                Debug.WriteLine(base64String);
            }
        }

        private void GetImagesId(int issueId)
        {
            try
            {
                var imageList = App.Client.GetImageInfo(issueId);
                foreach (var image in imageList)
                {
                    this.AddTheImages(image.ImageIssueId);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
            IsBusy = false;
        }
        #endregion

        #region ImageGallery

        public ObservableCollection<ImageModel> Images
        {
            get { return images; }
            set
            {
                images = value;
            }
        }

        public ImageSource PreviewImage { get; set; }

        public ICommand CameraCommand
        {
            get
            {
                return cameraCommand ?? new Command(async () => await ExecuteCameraCommand(), CanExecuteCameraCommand);
            }
        }

        public ICommand PickCommand
        {
            get
            {
                return pickCommand ?? new Command(async () => await ExecutePickCommand(), CanExecutePickCommand);
            }
        }

        public Guid PreviewId { get; set; }

        private readonly ICommand cameraCommand = null;
        private readonly ICommand pickCommand = null;
        private readonly ICommand previewImageCommand = null;

        private ObservableCollection<ImageModel> images = new ObservableCollection<ImageModel>();

        private ICommand PreviewImageCommand
        {
            get
            {
                return previewImageCommand ?? new Command<Guid>(img =>
                {
                    if (images.Count > 0)
                    {
                        var image = images.Single(x => x.ImageId == img).OrgImage;
                        if (image.Length > 0)
                        {
                            PreviewId = img;
                            PreviewImage = ImageSource.FromStream(() => new MemoryStream(image));
                        }
                    }
                });
            }
        }


        private bool CanExecuteCameraCommand()
        {
            return CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported;
        }

        private bool CanExecutePickCommand()
        {
            return CrossMedia.Current.IsPickPhotoSupported;
        }

        private async Task ExecutePickCommand()
        {
            MediaFile file = await CrossMedia.Current.PickPhotoAsync();

            if (file == null)
                return;

            byte[] imageAsBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                file.GetStream().CopyTo(memoryStream);
                file.Dispose();
                imageAsBytes = memoryStream.ToArray();
            }

            if (imageAsBytes.Length > 0)
            {
                IImageResizer resizer = DependencyService.Get<IImageResizer>();
                imageAsBytes = resizer.ResizeImage(imageAsBytes, 1080, 1080);

                ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
                Images.Add(new ImageModel { Source = imageSource, OrgImage = imageAsBytes });

                string base64String = Convert.ToBase64String(imageAsBytes);
                Debug.WriteLine(base64String);
            }
        }

        private async Task ExecuteCameraCommand()
        {
            MediaFile file = await CrossMedia.Current.TakePhotoAsync(
                new StoreCameraMediaOptions { PhotoSize = PhotoSize.Small });

            if (file == null)
                return;

            byte[] imageAsBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                file.GetStream().CopyTo(memoryStream);
                file.Dispose();
                imageAsBytes = memoryStream.ToArray();
            }

            if (imageAsBytes.Length > 0)
            {
                IImageResizer resizer = DependencyService.Get<IImageResizer>();
                imageAsBytes = resizer.ResizeImage(imageAsBytes, 1080, 1080);

                string base64String = Convert.ToBase64String(imageAsBytes);
                Debug.WriteLine(base64String);

                ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
                Images.Add(new ImageModel { Source = imageSource, OrgImage = imageAsBytes });
            }


        }

        #endregion

        #region Map



        #region Model

        private bool isClusteringEnabled;


        private async void GetLocation()
        {
            await GetCurrentLocationAsync();
        }


        public bool IsClusteringEnabled
        {
            get => isClusteringEnabled;
            set
            {
                isClusteringEnabled = value;
                RaisePropertyChanged(nameof(IsClusteringEnabled));
            }
        }
        public IRendererFunctions MapFunctions { get; set; }

        public TKTileUrlOptions TilesUrlOptions { get; set; }

        public MapSpan MapRegion { get; set; } = MapSpan.FromCenterAndRadius(new TK.CustomMap.Position(56.8790, 14.8059), Distance.FromKilometers(2));

        public ObservableCollection<TKCustomMapPin> Pins { get; set; } = new ObservableCollection<TKCustomMapPin>();

        public ObservableCollection<TKCircle> Circles { get; set; } = new ObservableCollection<TKCircle>();

        public ObservableCollection<TKPolyline> Lines { get; set; }

        public TK.CustomMap.Position MapCenter { get; set; }

        public TKCustomMapPin SelectedPin { get; set; }

        public string MapText { get; set; }

        #endregion
        public Command<TK.CustomMap.Position> MapLongPressCommand
        {
            get
            {
                return new Command<TK.CustomMap.Position>(position =>
                {

                    TKCustomMapPin pin = new TKCustomMapPin
                    {
                        Position = position,
                        Title = string.Format("Pin {0}, {1}", position.Latitude, position.Longitude),
                        ShowCallout = true,
                        IsDraggable = true
                    };
                    Pins.Clear();
                    Pins.Add(pin);
                });
            }
        }

        public Command<IPlaceResult> PlaceSelectedCommand
        {
            get
            {
                return new Command<IPlaceResult>(async p =>
                {
                    switch (p)
                    {
                        case GmsPlacePrediction gmsResult:
                            GmsDetailsResult details = await GmsPlace.Instance.GetDetails(gmsResult.PlaceId);
                            MapCenter = new TK.CustomMap.Position(details.Item.Geometry.Location.Latitude, details.Item.Geometry.Location.Longitude);
                            return;
                        case OsmNominatimResult osmResult:
                            MapCenter = new TK.CustomMap.Position(osmResult.Latitude, osmResult.Longitude);
                            return;
                    }

                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                            {
                                TKNativeAndroidPlaceResult prediction = (TKNativeAndroidPlaceResult)p;

                                TKPlaceDetails details = await TKNativePlacesApi.Instance.GetDetails(prediction.PlaceId);

                                MapCenter = details.Coordinate;
                                break;
                            }

                        case Device.iOS:
                            {
                                TKNativeiOSPlaceResult prediction = (TKNativeiOSPlaceResult)p;

                                MapCenter = prediction.Details.Coordinate;
                                break;
                            }
                    }
                });
            }
        }

        public Command PinSelectedCommand
        {
            get
            {
                return new Command<TKCustomMapPin>(pin =>
                {
                    MapRegion = MapSpan.FromCenterAndRadius(SelectedPin.Position, Distance.FromKilometers(1));
                });
            }
        }

        public Command ClearMapCommand
        {
            get
            {
                return new Command(() =>
                {
                    Pins.Clear();
                });
            }
        }


        private readonly ICommand initMapCommand = null;

        public ICommand InitMapCommand
        {
            get { return initMapCommand ?? new Command(async () => await GetCurrentLocationAsync()); }
        }

        private void AddPin(TK.CustomMap.Position position)
        {
            TKCustomMapPin pin = new TKCustomMapPin
            {
                Position = position,
                Title = string.Empty,
                ShowCallout = false
            };
            Pins.Clear();
            Pins.Add(pin);
        }

        private async Task GetCurrentLocationAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            TimeSpan timeSpan = TimeSpan.FromTicks(120 * 1000);
            Position position = new Position();
            try
            {
                MapText = "Searching for GPS location...";
                position = await this.geolocator.GetPositionAsync(timeSpan);
                if (position != null)
                {
                    App.Client.Issue.Latitude = position.Latitude;
                    App.Client.Issue.Longitude = position.Longitude;
                    MapCenter = new TK.CustomMap.Position(position.Latitude, position.Longitude);
                    MapRegion = MapSpan.FromCenterAndRadius(MapCenter, Distance.FromKilometers(2));
                    TK.CustomMap.Position x = new TK.CustomMap.Position(position.Latitude, position.Longitude);
                    MapRegion = MapSpan.FromCenterAndRadius(new TK.CustomMap.Position(x.Latitude, x.Longitude), Distance.FromKilometers(2));
                    AddPin(x);
                    UpdateGpsLocationText(x);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                MapText = "Unable to find position!";
            }

            IsBusy = false;
        }

        private void UpdateGpsLocationText(TK.CustomMap.Position position)
        {
            string text = string.Format("{0} x {1}", position.Longitude, position.Latitude);
            MapText = text;
        }
        #endregion
    }
}

