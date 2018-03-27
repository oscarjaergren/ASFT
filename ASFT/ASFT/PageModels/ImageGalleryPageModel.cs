﻿namespace ASFT.PageModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using ASFT.IServices;
    using IssueBase.Issue;
    using Plugin.Media;
    using Plugin.Media.Abstractions;
    using Xamarin.Forms;

    public class ImageGalleryPageModel
    {
        public ObservableCollection<ImageModel> Images
        {
            get { return images; }
        }

        public ImageSource PreviewImage
        {
            get { return previewImage; }
            set { previewImage = value; }
        }

        public ICommand CameraCommand
        {
            get
            {
                return cameraCommand ?? new Command(async () => await ExecuteCameraCommand(),
                           CanExecuteCameraCommand);
            }
        }

        public ICommand PickCommand
        {
            get
            {
                return pickCommand ??
                       new Command(async () => await ExecutePickCommand(), CanExecutePickCommand);
            }
        }

        public Guid PreviewId { get; set; }

        private readonly ICommand cameraCommand = null;
        private readonly ICommand pickCommand = null;
        private readonly ICommand previewImageCommand = null;

        private ImageSource previewImage;

        private string CheckForImagesText { get; set; }

        private int ImageLoadSize { get; set; }


        private bool AbortGettingImages { get; }

        private bool IsGettingsImages { get; set; }

        private bool CheckForImages { get; set; }


        private ObservableCollection<ImageModel> images = new ObservableCollection<ImageModel>();

        public ImageGalleryPageModel(ObservableCollection<IssuePageModel> issueImage, int imageLoadSize)
        {
            ImageLoadSize = imageLoadSize;
            AbortGettingImages = false;
            IsGettingsImages = false;
            CheckForImages = true;

            // Get Images from Issues.
            AbortGettingImages = false;
            if (IsGettingsImages)
                return;


            if (!CheckForImages) return;
            CheckForImages = false;
            App.Client.RunInBackground(RefreshImages);
        }

        public ImageGalleryPageModel(int imageLoadSize)
        {
            ImageLoadSize = imageLoadSize;
        }

        public ImageGalleryPageModel()
        {
        }

        #region Model

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

        #endregion

        public void LoadImages(int itemId)
        {
            // IsBusy = true;

            // List<GalleryImage> list = await FileHelper.LoadImages(Section, ItemId);
            // foreach (GalleryImage imageGallery in list)
            // {
            // Images.Add(imageGallery);
            // OnPropertyChanged("Images");
            // }

            // if (Images.Count == 0)
            // {
            // ShowEmpty = true;
            // ShowContent = false;
            // }
            // else
            // {
            // ShowEmpty = false;
            // ShowContent = true;
            // }

            // IsBusy = false;
        }

        private void RefreshImages()
        {
            if (CheckForImages)
            {
                CheckForImages = false;
                App.Client.RunInBackground(RefreshImages);
            }

            if (IsGettingsImages)
                return;

            IsGettingsImages = true;

            int maxImageSize = ImageLoadSize;

            // foreach (ImagePageModel item in Items)
            // {
            // if (AbortGettingImages) break;

            // if (item.IsImageUpdate == false)
            // {
            // string imgPath = App.Client.GetThumbnail(item.Id, item.IssueId, maxImageSize, false).Result;
            // if (imgPath.Length == 0)
            // {
            // Device.BeginInvokeOnMainThread(() =>
            // {
            // CheckForImagesText = "Downloading picture (image id: " + item.Id + ")";
            // });

            // imgPath = App.Client.GetThumbnail(item.Id, item.IssueId, maxImageSize, true).Result;
            // GC.Collect();
            // }

            // if (imgPath.Length > 0) item.LocalImagePath = imgPath;
            // }
            // }
            IsGettingsImages = false;

            Device.BeginInvokeOnMainThread(() => { CheckForImagesText = string.Empty; });
        }

        private bool CanExecuteCameraCommand()
        {
            return CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported;
        }

        private bool CanExecutePickCommand()
        {
            return CrossMedia.Current.IsPickPhotoSupported;
        }

        private void LoadImages(ObservableCollection<ImageModel> issueImage)
        {
            foreach (ImageModel item in issueImage)
                Images.Add(new ImageModel { Source = item.Source, OrgImage = item.OrgImage });
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

                ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
                Images.Add(new ImageModel { Source = imageSource, OrgImage = imageAsBytes });
            }
        }
    }
}