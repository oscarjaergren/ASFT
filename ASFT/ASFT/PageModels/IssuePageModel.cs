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
    using DataTypes.Enums;
    using FreshMvvm;
    using IssueBase.Issue;
    using Plugin.Geolocator;
    using Plugin.Geolocator.Abstractions;
    using Xamarin.Forms;

    [SuppressMessage("ReSharper", "InvertIf")]
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class IssuePageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Model

        private IGeolocator locator;

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


        public event PropertyChangedEventHandler PropertyChanged;

        public List<IssueSeverityModel> SeverityValues { get; set; }

        public List<IssueStatusModel> StatusValues { get; set; }



        public string StatusText
        {
            get { return statusText; }
            set
            {
                if (statusText == value) return;
                statusText = value;
                NotifyPropertyChanged();
            }
        }


        #region Properties

        public ImageGalleryPageModel ImageGalleryViewModel { get; set; } = new ImageGalleryPageModel();

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy == value) return;
                isBusy = value;
                NotifyPropertyChanged();
            }
        }

        public string ImageText
        {
            get { return imageText; }
            set
            {
                if (imageText == value) return;
                imageText = value;
                NotifyPropertyChanged();
            }
        }

        public IssueSeverity SeverityEx
        {
            get { return App.Client.Issue.Severity; }
            set
            {
                App.Client.Issue.Severity = value;
                NotifyPropertyChanged($"SeverityImagePath");
                App.Client.Issue.Changed = true;
            }
        }

        public IssueStatus StatusEx
        {
            get { return App.Client.Issue.Status; }
            set
            {
                App.Client.Issue.Status = value;
                NotifyPropertyChanged($"StatusImagePath");
                App.Client.Issue.Changed = true;
            }
        }

        public string LocationText
        {
            get { return App.Client.GetCurrentLocationName(); }
            set
            {
                locationText = value;
                NotifyPropertyChanged();
            }
        }

        public string TitleEx
        {
            get { return App.Client.Issue.Title; }
            set
            {
                if (App.Client.Issue.Title == value) return;
                App.Client.Issue.Title = value;
                NotifyPropertyChanged();
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
                NotifyPropertyChanged();
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
                NotifyPropertyChanged();
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
                NotifyPropertyChanged();
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
                    NotifyPropertyChanged(nameof(Severity5Opacity));
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
                    NotifyPropertyChanged(nameof(Severity4Opacity));
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
                    NotifyPropertyChanged(nameof(Severity3Opacity));
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
                    NotifyPropertyChanged(nameof(Severity2Opacity));
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
                    NotifyPropertyChanged(nameof(Severity1Opacity));
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
                    NotifyPropertyChanged(nameof(StatusUnresolvedOpacity));
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
                    NotifyPropertyChanged(nameof(StatusInProgressOpacity));
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
                    NotifyPropertyChanged(nameof(StatusDoneOpacity));
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

        public IssuePageModel()
        {
            if (App.Client.Issue == null)
            {
                App.Client.Issue = CreateIssueModel();
            }
            StatusValues = App.Client.Issue.PossibleStatusValues;
            SeverityValues = App.Client.Issue.PossibleSeverityValues;
            locator = CrossGeolocator.Current;
            this.GetLocationName();
            if (App.Client.Issue == null && App.Client.Issue.IsNewIssue) return;

            TitleEx = "New Event";
            SeverityEx = IssueSeverity.Medium;
            StatusEx = IssueStatus.InProgress;
            StatusChecker();
            GetLocation();


        }

        public override void Init(object initData)
        {
            UserDialogs.Instance.ShowLoading("Loading event...", MaskType.Clear);
            base.Init(initData);
            if (initData is IssueModel issue)
            {
                App.Client.Issue = issue;
                if (App.Client.Issue.ServerId != 0) this.GetImagesId(issue.ServerId);
            }

            UserDialogs.Instance.HideLoading();
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

            CreatedByEx = App.Client.GetCurrentUsername();
        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void GetLocation()
        {
            if (isBusy) return;
            isBusy = true;
            try
            {
                TimeSpan timeSpan = TimeSpan.FromTicks(120 * 1000);
                Position position = new Position();
                position = await this.locator.GetPositionAsync(timeSpan);
                App.Client.Issue.Latitude = position.Latitude;
                App.Client.Issue.Longitude = position.Longitude;
            }
            catch (Exception exception)
            {
                isBusy = false;
                Debug.WriteLine(exception);
            }

            isBusy = false;
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

        private void GetLocationName()
        {
            int LocationId = App.Client.GetCurrentLocationId();
            if (LocationId == -1)
            {
                App.Client.ShowSelectLocation();
            }
        }


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
                    ImageModel newimage = App.Client.GetImageInfo(imageId);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ImageText = "Image Uploaded successful";
                        ImageGalleryViewModel.Images.Add(newimage);
                        App.Client.RunInBackground(RefreshImages);
                    });
                    break;
                case UploadImageEvent.ImageCaptured:
                    break;
            }
        }

        private void RefreshImages()
        {
            if (ImageGalleryViewModel.Images.Count == 0) return;

            foreach (ImageModel item in ImageGalleryViewModel.Images)
            {
                AddTheImages(item.ImageIssueId);

                Device.BeginInvokeOnMainThread(() =>
                    {
                        this.ImageText = "Downloading picture (image id: " + item.ImageId + ")";
                    });
            }
            IsBusy = false;
            Device.BeginInvokeOnMainThread(() => { ImageText = string.Empty; });
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
                ImageGalleryViewModel.Images.Clear();

                var imageList = App.Client.GetImages(issueId);
                foreach (ImageModel image in imageList)
                {
                    ImageGalleryViewModel.Images.Add(image);
                }
                this.RefreshImages();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
            IsBusy = false;

        }

        #endregion
    }
}

