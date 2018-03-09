using System;
using System.IO;
using System.Threading.Tasks;
using ASFT.Client;
using ASFT.IServices;

#if WINDOWS_PHONE_APP
  using Windows.Storage;
#endif


[assembly: Xamarin.Forms.Dependency(typeof(FileHelper))]
namespace ASFT.Client
{
    internal class FileHelper : IFileHelper
    {
        public void Save(string name, string data)
        {
            Save_Droid(name, data);
        }
        public async Task<byte[]> ReadAll(string filename)
        {
            return await ReadAll_Droid(filename);
        }

        public async Task<string> Load(string name)
        {
            return await Load_Droid(name);
        }

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

        public bool DeleteFolder(string filepath, bool bRecursive)
        {
            if (Directory.Exists(filepath) == false)
                return true;

            Directory.Delete(filepath, bRecursive);
            return true;
        }

        public bool MakeSureDirectoryExists(string path, bool bFilenameIncluded)
        {
            string folder = "";
            if (bFilenameIncluded)
                folder = Path.GetDirectoryName(path);
            else
                folder = path;

            if (Directory.Exists(folder))
                return true;

            Directory.CreateDirectory(folder);
            return true;
        }
        public void WriteFile(string path, byte[] data)
        {
            File.WriteAllBytes(path, data);
        }

        public void DeleteFile(string filepath)
        {
            File.Delete(filepath);
        }
        public void CopyFile(string source, string target)
        {
            try
            {
                File.Copy(source, target);
            }
            catch (Exception)
            {
                int x = 0;
                x++;
            }
        }

        public void MoveFile(string source, string target)
        {
            try
            {
                if (File.Exists(source))
                {
                    if (File.Exists(target))
                        File.Delete(target);

                    File.Move(source, target);
                }

            }
            catch (Exception)
            {
                int x = 0;
                x++;
            }
        }

        public bool Exists(string filename)
        {
            throw new NotImplementedException();
        }
    }

}
