﻿using System;
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
using DataTypes.Enums;
using FreshMvvm;
using IssueBase.Issue;
using Xamarin.Forms;

namespace ASFT.PageModels
{
    public class IssuePageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Model

        public ObservableCollection<ImageModel> Items = new ObservableCollection<ImageModel>();
        public static INavigation Navigation;
        private readonly ImageGalleryPageModel imageGalleryViewModel = new ImageGalleryPageModel();

        private int ImageLoadSize { get; set; }
        public IssueModel Issue { get; set; }
        public bool AllowPinMovment { get; set; }

        public GeoLocation Location { get; set; }
        public ObservableCollection<IssueModel> Issues { get; set; }
        private List<IssueStatusModel> StatusValues { get; set; }
        private List<IssueSeverityModel> SeverityValues { get; set; }

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

        private string imageText;
        private double statusUnresolvedOpacity;

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


        private readonly ICommand onGoToListCommand = null;
        private readonly ICommand submitCommand = null;
        private readonly ICommand onStatusTappedCommand = null;
        private double statusDoneOpacity;
        private double statusInProgressOpacity;
        private bool isBusy;

        public bool IsStatusUnresolvedActive { get; set; }

        public ICommand OnStatusTappedCommand
        {
            get { return onStatusTappedCommand ?? new Command<string>(OnStatusTapped); }
        }

        public ICommand OnGoToListCommand
        {
            get { return onGoToListCommand ?? new Command(OnGoToList); }
        }

        public ICommand SubmitCommand
        {
            get { return submitCommand ?? new Command(OnSubmit); }
        }

        public string StatusText { get; set; }

        #endregion

        #region Onstart

        public IssueModel CreateIssueModel()
        {
            return new IssueModel()
            {
                LocationId = 0,
                Id = 0,
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

        public IssuePageModel(string butssezImageText)
        {
            Issue = CreateIssueModel();
            StatusValues = Issue.PossibleStatusValues;
            SeverityValues = Issue.PossibleSeverityValues;

            if (!Issue.IsNewIssue) return;
            TitleEx = "New Event";
            SeverityEx = IssueSeverity.Medium;
            StatusEx = IssueStatus.Done;
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

        public void ResetOpacities()
        {
            StatusUnresolvedOpacity = 0.5;
            StatusInProgressOpacity = 0.5;
            statusDoneOpacity = 0.5;
        }

        private async void OnSubmit()
        {
            bool saved = await SaveChanges();
            if (saved)
            {
                MessagingCenter.Send(this, "refresh");


                ObservableCollection<ImageModel> uiIssues = imageGalleryViewModel.Images;

                foreach (ImageModel image in uiIssues)
                {

                    image.LocalImagePath = image.ImageId.ToString();
                    bool bDone = await App.Client.PhotoUpload(Issue.Id, OnCallback_UploadImage, image.OrgImage,
                        image.LocalImagePath);
                }

            }
            else
            {
                //await DisplayAlert("Save Failed", "Save failed", "OK");
            }
        }

        #region Save

        protected async Task<bool> SaveChanges()
        {
            IsBusy = true;
            try
            {
                // Add or update issue..
                await Task.Run(async () => { await App.Client.SaveIssue(Issue); });

                IsBusy = false;
                return true;
            }
            catch
            {
                IsBusy = false;
                return false;
            }
        }

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
                        imageGalleryViewModel.Images.Add(newimage);
                        App.Client.RunInBackground(RefreshImages);
                    });
                    break;
            }
        }

        #endregion


        #region Images

        private void RefreshImages()
        {

            if (IsBusy)
                return;

            IsBusy = true;

            int maxImageSize = ImageLoadSize;

            if (Items.Count == 0)
                return;

            IsBusy = true;

            foreach (ImageModel item in Items)
            {
                if (IsBusy)
                {
                    break;
                }

                if (item.IsImageUpdate == false)
                {

                    string imgPath = App.Client.GetThumbnail(item.Id, item.IssueId, maxImageSize, false).Result;
                    if (imgPath.Length == 0)
                    {
                        Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                        {
                            imageText = "Downloading picture (image id: " + item.Id.ToString() + ")";
                        });

                        imgPath = App.Client.GetThumbnail(item.Id, item.IssueId, maxImageSize, true).Result;
                        GC.Collect();
                    }

                    if (imgPath.Length > 0)
                    {
                        item.LocalImagePath = imgPath;
                    }

                }
            }

            IsBusy = false;

            Device.BeginInvokeOnMainThread(() => { imageText = ""; });

        }

        protected void RefreshImageList(int issueId)
        {
            try
            {
                imageGalleryViewModel.Images.Clear();

                var imageList = App.Client.GetImages(issueId);
                foreach (ImageModel image in imageList)
                {
                    string localOriginalFilePath = App.Client.GetImageFilePath(image.Id, image.IssueId);
                    IFileHelper fileHelper = DependencyService.Get<IFileHelper>();
                    if (fileHelper.Exists(localOriginalFilePath))
                    {
                        var imageAsBytes = fileHelper.ReadAll(localOriginalFilePath).Result;

                        IImageResizer resizer = DependencyService.Get<IImageResizer>();
                        imageAsBytes = resizer.ResizeImage(imageAsBytes, 1080, 1080);

                        ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
                        Items.Add(new ImageModel() { Source = imageSource, OrgImage = imageAsBytes });
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

        #endregion


        private async void OnGoToList()
        {
            
             int locationID = App.Client.GetCurrentLocationId();
            await CoreMethods.PushPageModel<IssueListPageModel>(locationID);
        }
    }
}

