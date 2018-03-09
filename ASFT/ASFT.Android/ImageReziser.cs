using System.IO;
using Android.Graphics;
using ASFT.Droid;
using ASFT.IServices;

[assembly: Xamarin.Forms.Dependency(typeof(ImageResizer))]


namespace ASFT.Droid
{
    public class ImageResizer : IImageResizer
    {
            public byte[] ResizeImage(byte[] imageData)
            {
                if (imageData.Length > 0)
                {
                    Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        originalImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                        return ms.ToArray();
                    }
                }
                else
                {
                    return imageData;
                }
            }

            public byte[] ResizeImage(byte[] imageData, float width, float height)
            {
                Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
                Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, false);

                using (MemoryStream ms = new MemoryStream())
                {
                    resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                    return ms.ToArray();
                }
            }
        }
    }
