using System;
using System.IO;
using Android.Graphics;
using ASFT.Droid;
using ASFT.IServices;

[assembly: Xamarin.Forms.Dependency(typeof(ImageResizer))]

namespace ASFT.Droid
{
    public class ImageResizer : IImageResize
    {
        public ImageResizer()
        {
        }

        public byte[] ResizeImage(byte[] imageData, float width, float height)
        {
            // Load the bitmap
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, false);

            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                return ms.ToArray();
            }
        }

        public byte[] ResizeImageAndroid(byte[] imageData, float width, float height)
        {
            // Load the bitmap
            try
            {
                if (width <= 0 || height <= 0)
                    throw new Exception("Invalid image size. width / height can not be 0");

                Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
                Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, false);

                using (MemoryStream ms = new MemoryStream())
                {
                    resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 85, ms);
                    var bitmapArray = ms.ToArray();

                    if (bitmapArray.Length == 0)
                        throw new Exception("Invalid image. Size 0");

                    originalImage.Recycle();
                    resizedImage.Recycle();
                    return bitmapArray;
                }
            }
            catch (Exception /*ex*/)
            {
                int x = 0;
                x++;
                throw;
            }
        }
    }
}
