using System;
using System.Threading.Tasks;
using ASFT.Client;

namespace ASFT.IServices
{
    public interface IFileHelper
    {
        void Save(string name, string data);
        Task<string> Load(string name);

        Task<byte[]> ReadAll(string filename);

        bool Exists(string filename);
        bool MakeSureDirectoryExists(string path, bool bFilenameIncluded);
        void WriteFile(string path, byte[] data);
        void DeleteFile(string filepath);

        bool DeleteFolder(string filepath, bool bRecursive);
        void CopyFile(string source, string target);
        void MoveFile(string source, string target);
    }

    public interface ICameraHelper
    {
        // TODO - Change so it return a BYTE Array.
        Task<string> TakePhoto();
        Task<string> PickExistingPicture();
    }

    public interface IThreadHelper
    {
        void RunInBackground(Action action);
    }

    public interface IDirectoryService
    {
        string GetImagesCacheDirectory();
    }

    public interface IImageResizer
    {
        byte[] ResizeImage(byte[] imageData, float width, float height);
    }
}