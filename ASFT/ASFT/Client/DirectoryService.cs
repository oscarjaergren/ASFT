using ASFT.Client;
using ASFT.IServices;

#if __IOS__
 using Foundation;
#endif

[assembly: Xamarin.Forms.Dependency(typeof(DirectoryService))]

namespace ASFT.Client
{
    internal class DirectoryService : IDirectoryService
  {
    public string GetImagesCacheDirectory()
    {
      var imageCacheDirectory = string.Empty;

#if __IOS__
			if (UIKit.UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) 
			{
				var docs = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User) [0];
                imageCacheDirectory = docs.Path;
			} 
			else 
			{
                imageCacheDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}

#elif __ANDROID__

          var docsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
          imageCacheDirectory = Path.Combine (new string[] {docsDir, "..", "Library", "Caches", "DownloadedImages"});

          if (Directory.Exists(imageCacheDirectory) == false)
          {
              Directory.CreateDirectory(imageCacheDirectory);
          }
#elif WINDOWS_PHONE_APP
      int x = 0;
      x++;
#endif

          return imageCacheDirectory;
    }
  }

}
