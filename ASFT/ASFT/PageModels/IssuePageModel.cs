using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using ASFT.IServices;
using DataTypes.Enums;
using FreshMvvm;
using IssueBase.Issue;
using Xamarin.Forms;

namespace ASFT.PageModels
{
    public class IssuePageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Model

        public ObservableCollection<ImageModel> Images = new ObservableCollection<ImageModel>();
        public static INavigation Navigation;

        public IssueModel Issue { get; set; }

        private List<IssueStatusModel> StatusValues { get; set; }
        private List<IssueSeverityModel> SeverityValues { get; set; }
        public string StatusText { get; set; }
        private string imageText;
        private double statusUnresolvedOpacity;
        public int Opacity { get; set; }

        public new event PropertyChangedEventHandler PropertyChanged;

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
                NotifyPropertyChanged("SeverityImagePath");
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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                    NotifyPropertyChanged(nameof(StatusUnresolvedOpacity));
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
                    NotifyPropertyChanged(nameof(StatusUnresolvedOpacity));
                }
            }
        }
        public string StatusDone
        {
            get { return "statusDone.png"; }
        }

        public string StatusInProgress
        {
            get { return "statusInProgress.png"; }
        }

        public string StatusUnresolved
        {
            get { return "statusUnresolved.png"; }
        }


        #region Command


        private readonly ICommand onGoToListCommand = null;
        private readonly ICommand submitCommand = null;
        private readonly ICommand onStatusClickedCommand = null;
        private double statusDoneOpacity;
        private double statusInProgressOpacity;
        private bool isBusy;

        public bool IsStatusUnresolvedActive { get; set; }

        public ICommand OnStatusClickedCommand
        {
            get { return onStatusClickedCommand ?? new Command<string>(OnStatusTappeds); }
        }

        public ICommand OnGoToListCommand
        {
            get { return onGoToListCommand ?? new Command(OnGoToList); }
        }

        public ICommand SubmitCommand
        {
            get { return submitCommand ?? new Command(OnSubmit); }
        }


        #endregion


        #endregion

        #region Onstart

        public IssueModel CreateIssueModel()
        {
            return new IssueModel()
            {
                LocationId = 0,
                ServerId = 0,
                Title = "",
                Description = "",
                CreatedBy = App.Client.GetCurrentUsername(),
                IsNewIssue = true,
                Created = DateTime.Now,
                Edited = DateTime.Now,
            };
        }

        public IssuePageModel()
        {
            Issue = CreateIssueModel();
            StatusValues = Issue.PossibleStatusValues;
            SeverityValues = Issue.PossibleSeverityValues;

            if (!Issue.IsNewIssue) return;
            TitleEx = "New Event";
            SeverityEx = IssueSeverity.Medium;
            StatusEx = IssueStatus.Done;
        }

        public override void Init(object initData)
        {
            UserDialogs.Instance.ShowLoading("Loading event...", maskType: MaskType.Clear);
            base.Init(initData);
            if (initData is IssueModel issue)
            {
                Issue = issue;
                if (Issue.ServerId != 0) GetImages(issue.ServerId);
            }
            UserDialogs.Instance.HideLoading();
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

        private async Task ShowLoginPage()
        {
            await CoreMethods.PushPageModel<LoginPageModel>();
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
                    statusDoneOpacity = 1;
                    StatusText = "Done";
                    break;
            }
        }

        public void OnStatusTapped(string fileName)
        {
            foreach (IssueStatusModel item in StatusValues)
            {
                if (item.Name != fileName) continue;
                Issue.Status = item.Status;
                StatusChecker();
                return;
            }
        }

        public void OnStatusTappeds(object sender)
        {
            StatusUnresolvedOpacity = 0.5;
            StatusInProgressOpacity = 0.5;
            statusDoneOpacity = 0.5;

            Image image = (Image)sender;
            image.Opacity = 1;

            var buttons = new string[StatusValues.Count];
            for (int n = 0; n < StatusValues.Count; ++n)
            {
                buttons[n] = StatusValues[n].Name;
            }

            if (!(image.Source is FileImageSource fileImageSource)) return;
            string fileName = fileImageSource.File;
            foreach (IssueStatusModel item in StatusValues)
            {
                if (item.Name == fileName)
                {
                    Issue.StatusEx = item.Status;
                    StatusChecker();
                    return;
                }
            }
        }

        public void ResetOpacities()
        {
            StatusUnresolvedOpacity = 0.5;
            StatusInProgressOpacity = 0.5;
            statusDoneOpacity = 0.5;
        }

       

        #region Save

        private  async void OnSubmit()
        {
            IsBusy = true;
            if (isBusy) return;

            bool saved = await SaveChanges();

            if (saved)
            {
                UserDialogs.Instance.Toast("Issue has been uploaded");
                var imagesinCollection = Images;

                foreach (ImageModel image in imagesinCollection)
                {
                    UserDialogs.Instance.Toast("Uploading" + image.ImageId.ToString());
                    image.FileName = image.ImageId.ToString();
                    await App.Client.PhotoUpload(Issue.ServerId, OnCallback_UploadImage, image.OrgImage, image.FileName);
                }
            }
            else
            {
                 CoreMethods.DisplayAlert("Save Failed", "Save failed", "OK");
            }
            IsBusy = false;
        }




        protected async Task<bool> SaveChanges()
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

        protected void OnCallback_UploadImage(UploadImageEvent eventId, int imageId)
        {
            switch (eventId)
            {
                case UploadImageEvent.ImageUploading:
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() => { ImageText = "Uploading Image..."; });
                    break;
                case UploadImageEvent.ImageUploadFailed:
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(() => { ImageText = "Failed to upload image"; });
                    break;
                case UploadImageEvent.ImageUploadSucess:
                    ImageModel newimage = App.Client.GetImageInfo(imageId);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ImageText = "Image Uploaded successful";
                        Images.Add(newimage);
                        App.Client.RunInBackground(RefreshImages);
                    });
                    break;
            }
        }

        private void RefreshImages()
        {
            int maxImageSize = Int32.MaxValue;
            if (Images.Count == 0) return;

            foreach (ImageModel item in Images)
            {
                if (IsBusy) return;

                if (item.IsImageUpdate != false) continue;
                string imgPath = App.Client.GetThumbnail(item.ImageIssueId, item.ImageIssueId, maxImageSize, false).Result;
                if (imgPath.Length == 0)
                {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread((Action)(() =>
                    {
                        ImageText = "Downloading picture (image id: " + item.ImageId.ToString() + ")";
                    }));

                    imgPath = App.Client.GetThumbnail(item.ImageIssueId, item.ImageIssueId, maxImageSize, true).Result;
                    GC.Collect();
                }
                if (imgPath.Length <= 0) continue;
                item.FileName = imgPath;
            }
            IsBusy = false;
            Device.BeginInvokeOnMainThread(() => { ImageText = ""; });
        }

        protected void GetImages(int issueId)
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                Images.Clear();

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
                        Images.Add(new ImageModel() { Source = imageSource, OrgImage = imageAsBytes });
                        //imageGalleryViewModel.LoadImages(Images);

                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }
        #endregion


        private async void OnGoToList()
        {
            int locationId = App.Client.GetCurrentLocationId();
            await CoreMethods.PushPageModel<IssueListPageModel>(locationId);
        }
    }
}

