using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using ASFT.IServices;
using ASFT.Models;
using ASFT.Pages;
using ASFT.ViewModels;
using ASFT.Views;
using DataTypes.Enums;
using FreshMvvm;
using IssueBase.Issue;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;

namespace ASFT.PageModels
{
    public class IssuePageModel : FreshBasePageModel
    {

        public static INavigation Navigation;


        private readonly ImageGalleryViewModel imageGalleryViewModel = new ImageGalleryViewModel();

        #region Model
        private bool AbortGettingImages { get; set; }
        private bool IsGettingsImages { get; set; }
        private int ImageLoadSize { get; set; }
        public IssueModel Issue { get; set; }
        public bool AllowPinMovment { get; set; }
        public GeoLocation Location { get; set; }
        public ObservableCollection<IssueModel> Issues { get; set; }
        private List<IssueStatusModel> StatusValues { get; }
        private List<IssueSeverityModel> SeverityValues { get; }

        public int Opacity { get; set; }

        public new event PropertyChangedEventHandler PropertyChanged;

        private bool isProcessing;

        #region Model

        public bool IsProcessing
        {
            get { return isProcessing; }
            set
            {
                if (isProcessing == value) return;
                isProcessing = value;
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
            get { return Issue.Severity; }
            set
            {
                Issue.Severity = value;
                NotifyPropertyChanged("SeverityImagePath");
                Changed = true;
            }
        }

        public IssueStatus StatusEx
        {
            get { return Issue.Status; }
            set
            {
                Issue.Status = value;
                NotifyPropertyChanged($"StatusImagePath");
                Changed = true;
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
                Changed = true;
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
                Changed = true;
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
                Changed = true;
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
                Changed = true;
            }
        }


        public string SeverityImagePath
        {
            get { return GetSeverityImage(Issue.Severity); }
        }

        public string StatusImagePath
        {
            get { return GetStatusImage(Issue.Status); }
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
            get { return Issue.IssueChanged; }
            set
            {
                Issue.IssueChanged = value;
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


        #endregion
        public IssueModel CreateIssueModel()
        {
            return new IssueModel()
            {
                LocationId = 0,
                Id = 0,
                Title = "",
                Description = "",
                CreatedBy = App.Client.GetCurrentUsername(),
                Created = DateTime.Now,
                Edited = DateTime.Now,
            };
        }

        public IssuePageModel()
        {
            if (App.Client.Initilized == false) App.Client.Init();

            Issue = CreateIssueModel();

            IGeolocator locator = CrossGeolocator.Current;
            if (locator.DesiredAccuracy != 100)
                locator.DesiredAccuracy = 100;

            GeoLocation location = App.Client.GetCurrentGeoLocation();
            StatusValues = Issue.PossibleStatusValues;
            SeverityValues = Issue.PossibleSeverityValues;
            if (!Issue.IsNewIssue) return;
            TitleEx = "New Event";
            SeverityEx = IssueSeverity.Medium;
            StatusEx = IssueStatus.Done;
            CreatedByEx = App.Client.GetCurrentUsername();

        }
        public override async void Init(object initData)
        {
            await CoreMethods.DisplayAlert("InitsAppearing", "", "Ok");
            if (App.Client.LoggedIn != true)
            {
                await ShowLoginPage();
            }
        }


        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            if (App.Client.LoggedIn != true)
            {
                await ShowLoginPage();
            }
        }

        private async Task ShowLoginPage()
        {
            MessagingCenter.Subscribe<LoginPage>(this, "OnLoginPageClosed", sender =>
            {
                MessagingCenter.Unsubscribe<LoginPage>(this, "OnLoggedIn");

                if (App.Client.LoggedIn == false) Issues.Clear();
            });
            await CoreMethods.PushPageModel<LoginPageModel>();
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
            bool saved = await SaveChanges();
            if (saved)
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


        private void OnGoToList()
        {
            MessagingCenter.Subscribe<LoginPage>(this, "OnLoginPageClosed", sender =>
            {
                MessagingCenter.Unsubscribe<LoginPage>(this, "OnLoggedIn");

                if (App.Client.LoggedIn == false) Issues.Clear();
            });
            //await Navigation.PushModalAsync(new LoginPage());
        }


    }
}