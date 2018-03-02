using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ASFT.IServices;
using ASFT.Models;
using ASFT.PageModels;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace ASFT.ViewModels
{
    public class ImageGalleryViewModel : ImageGalleryModel
    {
        private readonly ICommand cameraCommand = null;
        private readonly ICommand pickCommand = null;
        private readonly ICommand previewImageCommand = null;
        private ObservableCollection<ImageViewModel> images = new ObservableCollection<ImageViewModel>();

        public ImageGalleryViewModel(ObservableCollection<IssuePageModel> issueImage, int imageLoadSize)
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

        public ImageGalleryViewModel(int imageLoadSize)
        {
            ImageLoadSize = imageLoadSize;
        }

        public ImageGalleryViewModel()
        {
        }

        public ObservableCollection<ImageViewModel> Items { get; set; }


        private bool AbortGettingImages { get; }
        private bool IsGettingsImages { get; set; }
        private bool CheckForImages { get; set; }
        private string CheckForImagesText { get; set; }
        private int ImageLoadSize { get; set; }

        public ObservableCollection<ImageViewModel> Images
        {
            get { return images; }
            set { images = value; }
        }

        public ImageSource PreviewImage { get; set; }

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

        public ICommand PreviewImageCommand
        {
            get
            {
                return previewImageCommand ?? new Command<Guid>(img =>
                {
                    var image = Images.Single(x => x.ImageId == img).OrgImage;

                    PreviewImage = ImageSource.FromStream(() => new MemoryStream(image));
                });
            }
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

            foreach (ImageViewModel item in Items)
            {
                if (AbortGettingImages) break;

                if (item.IsImageUpdate == false)
                {
                    string imgPath = App.Client.GetThumbnail(item.Id, item.IssueId, maxImageSize, false).Result;
                    if (imgPath.Length == 0)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            CheckForImagesText = "Downloading picture (image id: " + item.Id + ")";
                        });

                        imgPath = App.Client.GetThumbnail(item.Id, item.IssueId, maxImageSize, true).Result;
                        GC.Collect();
                    }

                    if (imgPath.Length > 0) item.LocalImagePath = imgPath;
                }
            }

            IsGettingsImages = false;

            Device.BeginInvokeOnMainThread(() => { CheckForImagesText = ""; });
        }

        public bool CanExecuteCameraCommand()
        {
            return CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported;
        }

        public bool CanExecutePickCommand()
        {
            return CrossMedia.Current.IsPickPhotoSupported;
        }

        public void LoadImages(ObservableCollection<ImageViewModel> issueImage)
        {
            foreach (ImageViewModel item in issueImage)
                Images.Add(new ImageViewModel {Source = item.Source, OrgImage = item.OrgImage});
        }


        public async Task ExecutePickCommand()
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
                IImageResize resizer = DependencyService.Get<IImageResize>();
                imageAsBytes = resizer.ResizeImage(imageAsBytes, 1080, 1080);

                ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
                Images.Add(new ImageViewModel {Source = imageSource, OrgImage = imageAsBytes});
            }
        }


        public async Task ExecuteCameraCommand()
        {
            MediaFile file = await CrossMedia.Current.TakePhotoAsync(
                new StoreCameraMediaOptions {PhotoSize = PhotoSize.Small});

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
                IImageResize resizer = DependencyService.Get<IImageResize>();
                imageAsBytes = resizer.ResizeImage(imageAsBytes, 1080, 1080);

                ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
                Images.Add(new ImageViewModel {Source = imageSource, OrgImage = imageAsBytes});
            }
        }
    }
}