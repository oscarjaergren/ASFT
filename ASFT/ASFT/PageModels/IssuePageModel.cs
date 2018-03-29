namespace ASFT.PageModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Acr.UserDialogs;
    using ASFT.IServices;
    using ASFT.Pages;

    using DataTypes.Enums;
    using FreshMvvm;
    using IssueBase.Issue;
    using Plugin.Geolocator;
    using Plugin.Geolocator.Abstractions;

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
    public class IssuePageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Model

        private IGeolocator geolocator;

        private bool isBusy;
        private string imageText;
        private string locationText;
        private string statusText;

        private double statusUnresolvedOpacity;
        private double statusInProgressOpacity;
        private double statusDoneOpacity;
        private double severity5Opacity;
        private double severity4Opacity;
        private double severity3Opacity;
        private double severity2Opacity;
        private double severity1Opacity;

        private ICommand onGoToListCommand;
        private ICommand submitCommand;
        private ICommand onStatusClickedCommand;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        public List<IssueSeverityModel> SeverityValues { get; set; }

        public List<IssueStatusModel> StatusValues { get; set; }


        // public ImageGalleryPageModel ImageGalleryViewModel = new ImageGalleryPageModel();
        public ImageGalleryPageModel ImageGalleryViewModel { get; set; } = new ImageGalleryPageModel();


        public string StatusText
        {
            get { return statusText; }
            set
            {
                if (statusText == value) return;
                statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy == value) return;
                isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public string ImageText
        {
            get { return imageText; }
            set
            {
                if (imageText == value) return;
                imageText = value;
                OnPropertyChanged(nameof(ImageText));
            }
        }

        public IssueSeverity SeverityEx
        {
            get { return App.Client.Issue.Severity; }
            set
            {
                App.Client.Issue.Severity = value;
                OnPropertyChanged(nameof(SeverityEx));
                App.Client.Issue.Changed = true;
            }
        }

        public IssueStatus StatusEx
        {
            get { return App.Client.Issue.Status; }
            set
            {
                App.Client.Issue.Status = value;
                OnPropertyChanged(nameof(StatusEx));
                App.Client.Issue.Changed = true;
            }
        }

        public string LocationText
        {
            get { return App.Client.GetCurrentLocationName(); }
            set
            {
                locationText = value;
                OnPropertyChanged(nameof(LocationText));
            }
        }

        public string TitleEx
        {
            get { return App.Client.Issue.Title; }
            set
            {
                if (App.Client.Issue.Title == value) return;
                App.Client.Issue.Title = value;
                OnPropertyChanged(nameof(TitleEx));
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
                OnPropertyChanged(nameof(CreatedByEx));
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
                OnPropertyChanged(nameof(CreatedEx));
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
                OnPropertyChanged(nameof(DescriptionEx));
                App.Client.Issue.Changed = true;
            }
        }
        #region Opacity Properties
        public double Severity5Opacity
        {
            get { return severity5Opacity; }
            set
            {
                if (severity5Opacity != value)
                {
                    severity5Opacity = value;
                    OnPropertyChanged(nameof(Severity5Opacity));
                }
            }
        }

        public double Severity4Opacity
        {
            get { return severity4Opacity; }
            set
            {
                if (severity4Opacity != value)
                {
                    severity4Opacity = value;
                    OnPropertyChanged(nameof(Severity4Opacity));
                }
            }
        }

        public double Severity3Opacity
        {
            get { return severity3Opacity; }
            set
            {
                if (severity3Opacity != value)
                {
                    severity3Opacity = value;
                    OnPropertyChanged(nameof(Severity3Opacity));
                }
            }
        }

        public double Severity2Opacity
        {
            get { return severity2Opacity; }
            set
            {
                if (severity2Opacity != value)
                {
                    severity2Opacity = value;
                    OnPropertyChanged(nameof(Severity2Opacity));
                }
            }
        }

        public double Severity1Opacity
        {
            get { return severity1Opacity; }
            set
            {
                if (severity1Opacity != value)
                {
                    severity1Opacity = value;
                    OnPropertyChanged(nameof(Severity1Opacity));
                }
            }
        }

        public double StatusUnresolvedOpacity
        {
            get { return statusUnresolvedOpacity; }
            set
            {
                if (statusUnresolvedOpacity != value)
                {
                    statusUnresolvedOpacity = value;
                    OnPropertyChanged(nameof(StatusUnresolvedOpacity));
                }
            }
        }

        public double StatusInProgressOpacity
        {
            get { return statusInProgressOpacity; }
            set
            {
                if (statusInProgressOpacity != value)
                {
                    statusInProgressOpacity = value;
                    OnPropertyChanged(nameof(StatusInProgressOpacity));
                }
            }
        }

        public double StatusDoneOpacity
        {
            get { return statusDoneOpacity; }
            set
            {
                if (statusDoneOpacity != value)
                {
                    statusDoneOpacity = value;
                    OnPropertyChanged(nameof(StatusDoneOpacity));
                }
            }
        }
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
            pins = new ObservableCollection<TKCustomMapPin>();
            circles = new ObservableCollection<TKCircle>();

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
                    mapRegion = MapSpan.FromCenterAndRadius(
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

        #endregion



        private async Task<bool> GetLocationName()
        {
            int LocationId = App.Client.GetCurrentLocationId();
            if (LocationId == -1)
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
            if (isBusy) return;
            IsBusy = true;

            bool saved = await SaveChanges();

            if (saved)
            {
                UserDialogs.Instance.Toast("Issue has been uploaded");
                var imagesinCollection = ImageGalleryViewModel.Images;

                foreach (ImageModel image in imagesinCollection)
                {
                    UserDialogs.Instance.Toast("Uploading" + image.ImageId);
                    image.Image.FileName = image.ImageId.ToString();
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
                        ImageGalleryViewModel.Images.Add(newimage);
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
                ImageGalleryViewModel.Images.Add(new ImageModel { Source = imageSource, OrgImage = imageAsBytes });
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

        #region Map



        #region Model
        private readonly Random random = new Random(1984);

        private TKTileUrlOptions tileUrlOptions;
        private MapSpan mapRegion = MapSpan.FromCenterAndRadius(new TK.CustomMap.Position(56.8790, 14.8059), Distance.FromKilometers(2));
        private TK.CustomMap.Position mapCenter;
        private TKCustomMapPin selectedPin;
        private bool isClusteringEnabled;
        private ObservableCollection<TKCustomMapPin> pins;
        private ObservableCollection<TKCircle> circles;
        private ObservableCollection<TKPolyline> lines;

        public TKTileUrlOptions TilesUrlOptions
        {
            get
            {
                return tileUrlOptions;

                // return new TKTileUrlOptions(
                // "http://a.basemaps.cartocdn.com/dark_all/{2}/{0}/{1}.png", 256, 256, 0, 18);
                // return new TKTileUrlOptions(
                // "http://a.tile.openstreetmap.org/{2}/{0}/{1}.png", 256, 256, 0, 18);
            }

            set
            {
                if (tileUrlOptions != value)
                {
                    tileUrlOptions = value;
                    OnPropertyChanged("TilesUrlOptions");
                }
            }
        }

        private async void GetLocation()
        {
            await GetCurrentLocationAsync();
        }

        public IRendererFunctions MapFunctions { get; set; }

        public bool IsClusteringEnabled
        {
            get => isClusteringEnabled;
            set
            {
                isClusteringEnabled = value;
                OnPropertyChanged(nameof(IsClusteringEnabled));
            }
        }

        public MapSpan MapRegion
        {
            get { return mapRegion; }
            set
            {
                if (mapRegion != value)
                {
                    mapRegion = value;
                    OnPropertyChanged("MapRegion");
                }
            }
        }

        public ObservableCollection<TKCustomMapPin> Pins
        {
            get { return pins; }
            set
            {
                if (pins != value)
                {
                    pins = value;
                    OnPropertyChanged("Pins");
                }
            }
        }

        public ObservableCollection<TKCircle> Circles
        {
            get { return circles; }
            set
            {
                if (circles != value)
                {
                    circles = value;
                    OnPropertyChanged("Circles");
                }
            }
        }
        public ObservableCollection<TKPolyline> Lines
        {
            get { return lines; }
            set
            {
                if (lines != value)
                {
                    lines = value;
                    OnPropertyChanged("Lines");
                }
            }
        }

        public TK.CustomMap.Position MapCenter
        {
            get { return mapCenter; }
            set
            {
                if (mapCenter != value)
                {
                    mapCenter = value;
                    OnPropertyChanged("MapCenter");
                }
            }
        }
        public TKCustomMapPin SelectedPin
        {
            get { return selectedPin; }
            set
            {
                if (selectedPin != value)
                {
                    selectedPin = value;
                    OnPropertyChanged("SelectedPin");
                }
            }
        }

        private string mapText;

        public string MapText
        {
            get { return mapText; }

            set
            {
                if (mapText != value)
                {
                    mapText = value;
                    OnPropertyChanged("MapText");
                }
            }
        }

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
                    pins.Clear();
                    pins.Add(pin);
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
                    pins.Clear();
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
            if (isBusy) return;
            isBusy = true;

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
                    mapRegion = MapSpan.FromCenterAndRadius(new TK.CustomMap.Position(x.Latitude, x.Longitude), Distance.FromKilometers(2));
                    AddPin(x);
                    UpdateGpsLocationText(x);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                MapText = "Unable to find position!";
            }

            isBusy = false;
        }

        private void UpdateGpsLocationText(TK.CustomMap.Position position)
        {
            string text = string.Format("{0} x {1}", position.Longitude, position.Latitude);
            MapText = text;
        }
        #endregion
    }
}

