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

        private readonly ObservableCollection<ImageModel> images = new ObservableCollection<ImageModel>();
        private readonly IGeolocator locator;

        private bool isBusy;
        private string imageText;
        private string locationText;
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

        public IssueModel Issue { get; set; }

        public List<IssueSeverityModel> SeverityValues { get; set; }

        public List<IssueStatusModel> StatusValues { get; set; }

        public string StatusText { get; set; }


        #region Properties


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
            get { return Issue.Severity; }
            set
            {
                Issue.Severity = value;
                NotifyPropertyChanged($"SeverityImagePath");
                Issue.Changed = true;
            }
        }

        public IssueStatus StatusEx
        {
            get { return Issue.Status; }
            set
            {
                Issue.Status = value;
                NotifyPropertyChanged($"StatusImagePath");
                Issue.Changed = true;
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
            get { return Issue.Title; }
            set
            {
                if (Issue.Title == value) return;
                Issue.Title = value;
                NotifyPropertyChanged();
                Issue.Changed = true;
            }
        }

        public string CreatedByEx
        {
            get { return Issue.CreatedBy; }
            set
            {
                if (Issue.CreatedBy == value) return;
                Issue.CreatedBy = value;
                NotifyPropertyChanged();
                Issue.Changed = true;
            }
        }

        public DateTime CreatedEx
        {
            get { return Issue.Created; }
            set
            {
                if (Issue.Created == value) return;
                Issue.Created = value;
                NotifyPropertyChanged();
                Issue.Changed = true;
            }
        }

        public string DescriptionEx
        {
            get { return Issue.Description; }
            set
            {
                if (Issue.Description == value) return;
                Issue.Description = value;
                NotifyPropertyChanged();
                Issue.Changed = true;
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
            Issue = CreateIssueModel();
            StatusValues = Issue.PossibleStatusValues;
            SeverityValues = Issue.PossibleSeverityValues;
            locator = CrossGeolocator.Current;
            if (!Issue.IsNewIssue) return;
            TitleEx = "New Event";
            GetLocation();
            SeverityEx = IssueSeverity.Medium;
            StatusEx = IssueStatus.InProgress;

            StatusChecker();
        }

        public override void Init(object initData)
        {
            UserDialogs.Instance.ShowLoading("Loading event...", MaskType.Clear);
            base.Init(initData);
            if (initData is IssueModel issue)
            {
                Issue = issue;
                if (Issue.ServerId != 0) GetImages(issue.ServerId);
                Issue.LocationId = App.Client.GetCurrentLocationId();
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
                Position position = await this.locator.GetPositionAsync(timeSpan);
                Issue.Latitude = position.Latitude;
                Issue.Longitude = position.Longitude;
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
            switch (Issue.Status)
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
            switch (Issue.Severity)
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
                    Issue.Status = item.Status;
                    StatusChecker();
                    return;
                }
            }

            foreach (IssueSeverityModel item in SeverityValues)
            {
                if (item.Name == (string)fileName)
                {
                    Issue.Severity = item.Severity;
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
                var imagesinCollection = this.images;

                foreach (ImageModel image in imagesinCollection)
                {
                    UserDialogs.Instance.Toast("Uploading" + image.ImageId);
                    image.FileName = image.ImageId.ToString();
                    await App.Client.PhotoUpload(Issue.ServerId, this.OnCallbackUploadImage, image.OrgImage, image.FileName);
                }
            }
            else
            {
                UserDialogs.Instance.Toast("Issue has been failed");
                UserDialogs.Instance.Alert("Save Failed", "Save failed", "OK");
            }

            UserDialogs.Instance.Toast("Issue has been failed");
            IsBusy = false;
        }

        private async Task<bool> SaveChanges()
        {
            try
            {
                await Task.Run(async () => { await App.Client.SaveIssue(Issue); });
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
                        this.images.Add(newimage);
                        App.Client.RunInBackground(RefreshImages);
                    });
                    break;
                case UploadImageEvent.ImageCaptured:
                    break;
            }
        }

        private void RefreshImages()
        {
            const int MaxImageSize = int.MaxValue;
            if (images.Count == 0) return;

            foreach (ImageModel item in this.images)
            {
                if (IsBusy) return;

                if (item.IsImageUpdate) continue;
                string imgPath = App.Client.GetThumbnail(item.ImageIssueId, item.ImageIssueId, MaxImageSize, false)
                    .Result;
                if (imgPath.Length == 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                        {
                            this.ImageText = "Downloading picture (image id: " + item.ImageId + ")";
                        });

                    imgPath = App.Client.GetThumbnail(item.ImageIssueId, item.ImageIssueId, MaxImageSize, true).Result;
                    GC.Collect();
                }

                if (imgPath.Length <= 0) continue;
                item.FileName = imgPath;
            }

            IsBusy = false;
            Device.BeginInvokeOnMainThread(() => { ImageText = string.Empty; });
        }

        private void GetImages(int issueId)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                this.images.Clear();

                var imageList = App.Client.GetImages(issueId);
                foreach (ImageModel image in imageList)
                {
                    string localOriginalFilePath = App.Client.GetImageFilePath(image.ImageIssueId, image.ImageIssueId);
                    IFileHelper fileHelper = DependencyService.Get<IFileHelper>();
                    if (fileHelper.Exists(localOriginalFilePath))
                    {
                        var imageAsBytes = fileHelper.ReadAll(localOriginalFilePath).Result;

                        IImageResizer resizer = DependencyService.Get<IImageResizer>();
                        imageAsBytes = resizer.ResizeImage(imageAsBytes, 1080, 1080);

                        ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
                        this.images.Add(new ImageModel { Source = imageSource, OrgImage = imageAsBytes });

                        // imageGalleryViewModel.LoadImages(Images);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        #endregion
    }
}

