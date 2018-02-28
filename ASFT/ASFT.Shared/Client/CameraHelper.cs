using System;
using System.Collections.Generic;
using System.Text;
using ASFT.IssueManager.Interfaces;
using ASFT.IssueManager.Client;
using System.IO;
using System.Threading.Tasks;
using ASFT.IssueManager.Shared.Client;

#if WINDOWS_PHONE_APP
  using Windows.Media.Capture;
  using Media.Plugin;
#endif
#if __ANDROID__ || __IOS__
using Plugin.Media;
#endif

[assembly: Xamarin.Forms.Dependency(typeof(CameraHelper))]

namespace ASFT.IssueManager.Shared.Client
{
    class CameraHelper : ICameraHelper
    {
      public Task<String> TakePhoto()
      {
#if WINDOWS_PHONE_APP
        return TakePhoto_WinPhone();
#endif

#if __ANDROID__ || __IOS__
        return TakePhoto_Droid();
#endif
      //  return Task.Run(() => "");
      }

      public Task<String> PickExistingPicture()
      {
#if WINDOWS_PHONE_APP
        return PickExistingPicture_WinPhone();
#endif
#if __ANDROID__ || __IOS__
        return PickExistingPicture_Droid();
#endif

      }

#if __ANDROID__ || __IOS__
      protected async Task<String> TakePhoto_Droid()
      {
        if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
        {
          return "";
        }

        try
        {
          var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions { Directory = "Sample", Name = "snap.jpg" });
          return file.Path;
        }
        catch(Exception /*ex*/)
        {
          // We get here if we press BACK inthe camera. Null object exception
          return "";
        }
      }
      
      protected async Task<String> PickExistingPicture_Droid()
      {
        if (!CrossMedia.Current.IsPickPhotoSupported)
          return "";

        var file = await CrossMedia.Current.PickPhotoAsync();
        return file.Path;
      }
#endif

#if WINDOWS_PHONE_APP
    protected async Task<String> TakePhoto_WinPhone()
    {

      try
      {
         /*
        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.Thumbnail;
        foreach (var filter in SupportedImageFileTypes)
          picker.FileTypeFilter.Add(filter);

        picker.PickSingleFileAndContinue();
        */
        bool bIsCameraSupported = await Task.Run( () => 
        {
          if(!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
          {
            return false;
          }
          return true;
        });


        if (bIsCameraSupported == false)
          return "";

        var file = await CrossMedia.Current.TakePhotoAsync(new Media.Plugin.Abstractions.StoreCameraMediaOptions { Directory = "Sample", Name = "snap.jpg" });

        return file.Path;
        /*
        if (CrossMedia.Current.IsPickPhotoSupported)
        {

          var file = await CrossMedia.Current.PickPhotoAsync();
          int x = 0;
          x++;
        }
         * */
      }
      catch (Exception /*ex*/)
      {
        int x = 0;
        x++;
        return "";
      }
        /*
      if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
      {
        return;
      }

      var file = await CrossMedia.Current.TakePhotoAsync(new Media.Plugin.Abstractions.StoreCameraMediaOptions { Directory = "Sample", Name = "snap.jpg" });
        */
    }

    protected async Task<String> PickExistingPicture_WinPhone()
    {
      try
      {
        bool bIsPhotoPickSupported = await Task.Run(() =>
        {
          if (CrossMedia.Current.IsPickPhotoSupported == false)
          {
            return false;
          }
          return true;
        });

        if (bIsPhotoPickSupported == false)
          return "";

        var file = await CrossMedia.Current.PickPhotoAsync();
        return file.Path;
      }
      catch(Exception /*ex*/)
      {
        return "";
      }
    }
#endif
    }
}
