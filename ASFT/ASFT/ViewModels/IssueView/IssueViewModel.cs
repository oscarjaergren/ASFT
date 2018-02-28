using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ASFT.HelperMethods;
using ASFT.IServices;
using ASFT.Models;
using ASFT.Views;
using DataTypes.Enums;
using FreshMvvm;
using IssueBase.Issue;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;

namespace ASFT.ViewModels
{
    public class IssueViewModel : IssueModel, INotifyPropertyChanged
    {
        public static INavigation Navigation;
        private bool bMapCtrlReady = false;

        private bool isGettingLocation = false;


        private ExtendedMap map;

        private ImageGalleryViewModel imageGalleryViewModel = new ImageGalleryViewModel();

        #region Model
        private bool AbortGettingImages { get; set; }
        private bool IsGettingsImages { get; set; }
        private int ImageLoadSize { get; set; }
        public IssueModel Issue { get; set; }
        public bool AllowPinMovment { get; set; }
        public GeoLocation Location { get; set; }
        public ObservableCollection<IssueViewModel> Issues { get; set; }
        private List<IssueStatusModel> StatusValues { get; }
        private List<IssueSeverityModel> SeverityValues { get; }

        public int Opacity { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsProcessing
        {
            get { return IsProcessing; }
            set
            {
                if (IsProcessing == value) return;
                IsProcessing = value;
                NotifyPropertyChanged();
                Changed = true;
            }
        }

        private string imageText;
        public string ImageText
        {
            get { return imageText; }
            set
            {
                if (imageText == value) return;
                imageText = value;
                NotifyPropertyChanged();
                Changed = true;
            }
        }


        public IssueSeverity SeverityEx
        {
            get { return Severity; }
            set
            {
                Severity = value;
                NotifyPropertyChanged("SeverityImagePath");
                Changed = true;
            }
        }

        public IssueStatus StatusEx
        {
            get { return Status; }
            set
            {
                Status = value;
                NotifyPropertyChanged($"StatusImagePath");
                Changed = true;
            }
        }

        public string TitleEx
        {
            get { return Title; }
            set
            {
                if (Title == value) return;
                Title = value;
                NotifyPropertyChanged();
                Changed = true;
            }
        }

        public string CreatedByEx
        {
            get { return CreatedBy; }
            set
            {
                if (CreatedBy == value) return;
                CreatedBy = value;
                NotifyPropertyChanged();
                Changed = true;
            }
        }

        public string DescriptionEx
        {
            get { return Description; }
            set
            {
                if (Description == value) return;
                Description = value;
                NotifyPropertyChanged();
                Changed = true;
            }
        }


        public string SeverityImagePath
        {
            get { return GetSeverityImage(Severity); }
        }

        public string StatusImagePath
        {
            get { return GetStatusImage(Status); }
        }

        public string GetSeverityImage(IssueSeverity severity)
        {
            switch (severity)
            {
                case IssueSeverity.Lowest:
                    return "severity_1.png";
                case IssueSeverity.Low:
                    return "severity_2.png";
                case IssueSeverity.Medium:
                    return "severity_3.png";
                case IssueSeverity.High:
                    return "severity_4.png";
                case IssueSeverity.Highest:
                    return "severity_5.png";
                default:
                    throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
            }
        }

        public string GetStatusImage(IssueStatus status)
        {
            switch (status)
            {
                case IssueStatus.Unresolved: return "statusUnresolved.png";
                case IssueStatus.InProgress: return "statusInProgress.png";
                case IssueStatus.Done: return "statusDone.png";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public bool Changed
        {
            get { return IssueChanged; }
            set
            {
                IssueChanged = value;
                NotifyPropertyChanged();
            }
        }

     

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // System.Diagnostics.Debug.WriteLine("Update!"); //ok
            //PropertyChanged is always null and shouldn't.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        public IssueViewModel()
        {

            //Id = 0;
            //Description = "";
            //Created = DateTime.Now;
            //Edited = DateTime.Now;
            //RefreshImageList(Issue.Id);

            GeoLocation location = App.Client.GetCurrentGeoLocation();


        }

    

        public IssueViewModel(IssueModel issue, GeoLocation location)
        {
            AllowPinMovment = false;
            Issue = Issue;
            Location = location;

            IGeolocator locator = CrossGeolocator.Current;
            if (locator.DesiredAccuracy != 100)
                locator.DesiredAccuracy = 100;


            Issue = issue;
            StatusValues = Issue.PossibleStatusValues;
            SeverityValues = Issue.PossibleSeverityValues;


            if (App.Client.Initilized == false) App.Client.Init();

            if (App.Client.LoggedIn != true) ShowLoginPage();

            if (IsNewIssue) 
            {
                Title = "New Event";
                Issue.Severity = IssueSeverity.Medium;
                Issue.Status = IssueStatus.Done;
                Issue.CreatedBy = App.Client.GetCurrentUsername();
            }
        }


      

        public IssueViewModel(bool bIsNew)
        {
            IsNewIssue = true;

            Id = 0;
            Title = "";
            Description = "";
            CreatedBy = App.Client.GetCurrentUsername();
            Created = DateTime.Now;
            Edited = DateTime.Now;
        }

        public IssueViewModel(IssueModel issue)
        {
            LocationId = issue.LocationId;
            Id = issue.Id;
            Title = issue.Title;
            Description = issue.Description;
            Longitude = issue.Longitude;
            Latitude = issue.Latitude;
            Status = issue.Status;
            Severity = issue.Severity;
            Created = issue.Created;
            Edited = issue.Edited;
            CreatedBy = issue.CreatedBy;
        }

        public IssueViewModel(int locationId, int id, string title, string desc, double longitude, double latitude,
            IssueStatus status, IssueSeverity severity)
        {
            LocationId = locationId;
            Id = id;
            Title = title;
            Description = desc;
            Longitude = longitude;
            Latitude = latitude;
            Status = status;
            Severity = severity;
        }

    

        private async Task ShowLoginPage()
        {
            MessagingCenter.Subscribe<LoginView>(this, "OnLoginPageClosed", sender =>
            {
                MessagingCenter.Unsubscribe<LoginView>(this, "OnLoggedIn");

                if (App.Client.LoggedIn == false) Issues.Clear();
            });
            await Navigation.PushModalAsync(new LoginView());
        }

      


        private readonly ICommand onGoToListCommand = null;
        private readonly ICommand onStatusTappedCommand = null;
        private readonly ICommand submitCommand = null;


        public ICommand OnStatusTappedCommand
        {
            get { return onStatusTappedCommand ?? new Command(OnGoToList); }
        }
        public ICommand OnGoToListCommand
        {
            get { return onGoToListCommand ?? new Command(OnGoToList); }
        }

        public ICommand SubmitCommand
        {
            get { return submitCommand ?? new Command(OnSubmit); }
        }

        private async void OnSubmit()
        {
            bool Saved = await SaveChanges();
            if (Saved)
            {
                MessagingCenter.Send(this, "refresh");


                ObservableCollection<ImageViewModel> uiIssues = imageGalleryViewModel.Images;

                foreach (var image in uiIssues)
                {

                    image.LocalImagePath = image.ImageId.ToString();
                    bool bDone = await App.Client.PhotoUpload(Issue.Id, OnCallback_UploadImage, image.OrgImage, image.LocalImagePath);
                }

            }
            else
            {
                //await DisplayAlert("Save Failed", "Save failed", "OK");
            }
        }


        protected async Task<bool> SaveChanges()
        {
            IsProcessing = true;
            try
            {
                // Add or update issue..
                await Task.Run(async () =>
                {
                    await App.Client.SaveIssue(Issue);
                });

                IsProcessing = false;
                return true;
            }
            catch
            {
                IsProcessing = false;
                return false;
            }
        }
        protected void OnCallback_UploadImage(UploadImageEvent eventId, int ImageId)
        {
            switch (eventId)
            {
                case UploadImageEvent.ImageUploading:
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                    {
                        ImageText = "Uploading Image...";
                    });
                    break;
                case UploadImageEvent.ImageUploadFailed:
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                    {
                        ImageText = "Failed to upload image";
                    });
                    break;
                case UploadImageEvent.ImageUploadSucess:
                    ImageViewModel newimage = App.Client.GetImageInfo(ImageId);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ImageText = "Image Uploaded successful";
                        imageGalleryViewModel.Images.Add(newimage);
                        App.Client.RunInBackground(RefreshImages);
                    });
                    break;
            }
        }
        public ObservableCollection<ImageViewModel> Items = new ObservableCollection<ImageViewModel>();

     

        protected void RefreshImageList(int issueId)
        {

            try
            {

                imageGalleryViewModel.Images.Clear();

                var ImageList = App.Client.GetImages(issueId);
                foreach (var image in ImageList)
                {
                    string localOriginalFilePath = App.Client.GetImageFilePath(image.Id, image.IssueId);
                    var FileHelper = DependencyService.Get<IFileHelper>();
                    if (FileHelper.Exists(localOriginalFilePath))
                    {
                        var imageAsBytes = FileHelper.ReadAll(localOriginalFilePath).Result;

                        var resizer = DependencyService.Get<IImageResize>();
                        imageAsBytes = resizer.ResizeImage(imageAsBytes, 1080, 1080);

                        var imageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
                        Items.Add(new ImageViewModel() { Source = imageSource, OrgImage = imageAsBytes });
                        imageGalleryViewModel.LoadImages(Items);

                    }
                }
            }
            catch (Exception)
            {
                int x = 0;
                x++;
            }
        }



        private void RefreshImages()
        {

            if (IsGettingsImages)
                return;

            IsGettingsImages = true;

            int MaxImageSize = ImageLoadSize;

            if (Items.Count == 0)
                return;

            IsGettingsImages = true;

            foreach (ImageViewModel item in Items)
            {
                if (AbortGettingImages)
                {
                    break;
                }

                if (item.IsImageUpdate == false)
                {

                    String imgPath = App.Client.GetThumbnail(item.Id, item.IssueId, MaxImageSize, false).Result;
                    if (imgPath.Length == 0)
                    {
                        Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                        {
                            imageText = "Downloading picture (image id: " + item.Id.ToString() + ")";
                        });

                        imgPath = App.Client.GetThumbnail(item.Id, item.IssueId, MaxImageSize, true).Result;
                        GC.Collect();
                    }

                    if (imgPath.Length > 0)
                    {
                        item.LocalImagePath = imgPath;
                    }

                }
            }

            IsGettingsImages = false;

            Device.BeginInvokeOnMainThread(() =>
            {
                imageText = "";
            });

        }


        private async void OnGoToList()
        {
            MessagingCenter.Subscribe<LoginView>(this, "OnLoginPageClosed", sender =>
            {
                MessagingCenter.Unsubscribe<LoginView>(this, "OnLoggedIn");

                if (App.Client.LoggedIn == false) Issues.Clear();
            });
            await Navigation.PushModalAsync(new LoginView());
        }


    }
}