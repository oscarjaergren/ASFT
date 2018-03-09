using System;
using ASFT.Client;
using ASFT.IServices;

#if __IOS__
using System.Drawing;
using UIKit;
using CoreGraphics;
using ImageIO;
using Foundation;
#endif

#if __ANDROID__
using Android.Graphics;
using Android.Media;
using System.Threading.Tasks;
#endif

#if WINDOWS_PHONE
using Microsoft.Phone;
using System.Windows.Media.Imaging;
#endif


[assembly: Xamarin.Forms.Dependency(typeof(ImageHelper))]
namespace ASFT.Client
{
  public class ImageHelper : IImageHelper
  {

    public ImageSize GetImageSize(string path)
    {
#if __IOS__
        return ImageSizeIos(path);
#endif
#if __ANDROID__
      return ImageSizeAndroid(path);
#endif
#if WINDOWS_PHONE  
      return ImageSizeWinPhone ( path );
#endif
#if WINDOWS_PHONE_APP
      return new ImageSize(0, 0);
#endif
        return null;
    }


#if __ANDROID__
    public ImageSize ImageSizeAndroid(string localPath)
    {

      BitmapFactory.Options options = new BitmapFactory.Options ();
	    options.InJustDecodeBounds = true;
	    var res = BitmapFactory.DecodeFileAsync (localPath, options);
      res.Wait();

      ImageSize sz = new ImageSize(options.OutWidth, options.OutHeight);
      if(res.Result != null)
      {
        res.Result.Recycle();
        res.Result.Dispose();
      }
      return sz;
    }
#endif
#if __IOS__
      public ImageSize ImageSizeIos(string localPath)
      {
        using (var src = CGImageSource.FromUrl(NSUrl.FromFilename(localPath)))
        {
          CGImageOptions options = new CGImageOptions() { ShouldCache = false };
          // do not forget the '0' image index or you won't get what you're looking for!
          var p = src.GetProperties(0, options);
          return new ImageSize((int)p.PixelWidth, (int)p.PixelHeight);
        }
      }
#endif
  }
}
