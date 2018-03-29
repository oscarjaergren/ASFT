namespace ASFT.PageModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
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
            set
            {
                images = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Images)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
       
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private ObservableCollection<ImageModel> images = new ObservableCollection<ImageModel>();


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