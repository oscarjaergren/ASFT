using ASFT.Droid;
using ASFT.IServices;

[assembly: Xamarin.Forms.Dependency(typeof(DirectoryService))]

namespace ASFT.Droid
{
    using System.IO;

    public class DirectoryService : IDirectoryService
    {
        public string GetImagesCacheDirectory()
        {
            string imageCacheDirectory = string.Empty;
            string docsDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            imageCacheDirectory = Path.Combine(new string[] { docsDir, "..", "Library", "Caches", "DownloadedImages" });

            if (Directory.Exists(imageCacheDirectory) == false)
            {
                Directory.CreateDirectory(imageCacheDirectory);
            }
            return imageCacheDirectory;
        }
    }

}
