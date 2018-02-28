using System;
using ASFT.IssueManager.Interfaces;
using System.IO;
using System.Threading.Tasks;
using ASFT.IssueManager.Shared.Client;

#if WINDOWS_PHONE_APP
  using Windows.Storage;
#endif


[assembly: Xamarin.Forms.Dependency(typeof(FileHelper))]
namespace ASFT.IssueManager.Shared.Client
{
    class FileHelper : IFileHelper
  {
    public void Save(String name, String data)
    {
#if __ANDROID__ || __IOS__
      Save_Droid(name, data);
#endif

#if WINDOWS_PHONE_APP
      Save_WinPhone(name, data);
#endif

    }
    public async Task<byte[]> ReadAll(string filename)
    {
#if __ANDROID__ || __IOS__

      return await ReadAll_Droid(filename);

#endif
        }

        public async Task<String> Load(String name)
    {
#if __ANDROID__ || __IOS__
      return await Load_Droid(name);
#endif

#if WINDOWS_PHONE_APP
      return await Load_WinPhone(name);

#endif



    }

#if __ANDROID__ || __IOS__
    protected Task<String> Load_Droid(String fileName)
    {
      return Task.Run(() =>
      {
        String content;
        String path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string fullFilename = Path.Combine(path, fileName);
        if (File.Exists(fullFilename) == false)
          return "";

        using (var streamReader = new StreamReader(fullFilename))
        {
          return content = streamReader.ReadToEnd();
        }
      });
    }

    protected Task<byte[]> ReadAll_Droid(string filename)
    {
      return Task.Run(() =>
      {
        return File.ReadAllBytes(filename);
      });
    }

    protected void Save_Droid(String fileName, String data)
    {
      string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
      string fullFilename = Path.Combine(path, fileName);

      using (var streamWriter = new StreamWriter(fullFilename, false))
      {
        streamWriter.Write(data);
      }

    }

#endif
#if WINDOWS_PHONE_APP
    protected async Task<String> Load_WinPhone(String fileName)
    {
      var folder = ApplicationData.Current.LocalFolder;

      try
      {
          var file = await folder.OpenStreamForReadAsync(fileName);

          using (var streamReader = new StreamReader(file))
          {
              return streamReader.ReadToEnd();
          }
      }
      catch (Exception /*ex*/)
      {
          return string.Empty;
      }
    }

    protected async void Save_WinPhone(String fileName, String data)
    {
      var folder = ApplicationData.Current.LocalFolder;

      try
      {
        var file = await folder.OpenStreamForWriteAsync(fileName, CreationCollisionOption.ReplaceExisting);

        using (var streamwriter = new StreamWriter(file))
        {
          streamwriter.Write(data);
        }
      }
      catch (Exception)
      {
        return;
      }
    }

#endif

    public bool Exists(String filename)
    {
#if WINDOWS_PHONE_APP
    try 
    {
      return Task.Run(async () =>
      {
        var file = await ApplicationData.Current.LocalCacheFolder.GetFileAsync(filename);
        return true;
      }).Result;

    }
    catch (FileNotFoundException) 
    {
      return false;
    }
#else
      return File.Exists(filename);
#endif
    }

    public bool DeleteFolder(String filepath, bool bRecursive)
    {
#if WINDOWS_PHONE_APP
     return false;
#else
      if (Directory.Exists(filepath) == false)
        return true;

      Directory.Delete(filepath, bRecursive);
      return true;
#endif
    }

    public bool MakeSureDirectoryExists(String path, bool bFilenameIncluded)
    {
#if WINDOWS_PHONE_APP
     return false;
#else
      String folder = "";
      if (bFilenameIncluded)
        folder = Path.GetDirectoryName(path);
      else
        folder = path;

      if (Directory.Exists(folder))
        return true;

      Directory.CreateDirectory(folder);
      return true;
#endif
    }
    public void WriteFile(String path, byte[] data)
    {
#if WINDOWS_PHONE_APP
     
#else
      File.WriteAllBytes(path, data);
#endif
    }


    public void DeleteFile(String filepath)
    {
#if WINDOWS_PHONE_APP
     
#else
      File.Delete(filepath);
#endif
    }

    public void CopyFile(String source, String target)
    {
#if WINDOWS_PHONE_APP
     
#else
      try
      {
        File.Copy(source, target);
      }
      catch(Exception)
      {
        int x = 0;
        x++;
      }
#endif
    }

    public void MoveFile(String source, String target)
    {
#if WINDOWS_PHONE_APP
     
#else
      try
      {
        if(File.Exists(source))
        {
          if (File.Exists(target))
            File.Delete(target);

          File.Move(source, target);
        }

      }
      catch(Exception)
      {
        int x = 0;
        x++;
      }
#endif
    }

  }

  //
}
