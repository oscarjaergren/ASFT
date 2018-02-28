using System;
using ASFT.IServices;
using Xamarin.Forms;

namespace ASFT.Client
{
    internal static class ThumbnailCreator
    {
        private static readonly object Lock = new object();

        public static void CreateThumbnail(string localOriginalFilePath, string localThumbnailFilePath,
            ImageSize maxSize)
        {
            lock (Lock)
            {
                IFileHelper fileHelper = DependencyService.Get<IFileHelper>();
                IImageHelper imageHelper = DependencyService.Get<IImageHelper>();
                IImageResizer imageResizer = DependencyService.Get<IImageResizer>();

                if (!fileHelper.Exists(localOriginalFilePath) || maxSize == null) return;
                ImageSize originalImageSize = imageHelper.GetImageSize(localOriginalFilePath);

                var bytesOriginalImage = fileHelper.ReadAll(localOriginalFilePath).Result;

                ImageSize thumbSize = GetThumbnailSize(originalImageSize, maxSize);

                var newImage = imageResizer.ResizeImage(bytesOriginalImage, thumbSize.Width, thumbSize.Height);
                if (newImage.Length > 0) fileHelper.WriteFile(localThumbnailFilePath, newImage);
            }
        }

        private static ImageSize GetThumbnailSize(ImageSize originalSize, ImageSize maxSize)
        {
            int width = originalSize.Width;
            int height = originalSize.Height;

            if (width > maxSize.Width)
            {
                width = maxSize.Width;

                decimal widthRatio = maxSize.Width / (decimal) originalSize.Width;
                height = (int) Math.Floor(originalSize.Height * widthRatio);
            }

            if (height <= maxSize.Height) return new ImageSize(width, height);
            height = maxSize.Height;

            decimal heightRatio = maxSize.Height / (decimal) originalSize.Height;
            width = (int) Math.Floor(originalSize.Width * heightRatio);

            return new ImageSize(width, height);
        }
    }
}