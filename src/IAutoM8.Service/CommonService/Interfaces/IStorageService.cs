using IAutoM8.Global.Enums;
using System.IO;
using System.Threading.Tasks;

namespace IAutoM8.Service.CommonService.Interfaces
{
    public interface IStorageService
    {
        string GetProfileImageUri(string fileName);
        string GetFileUri(string fileName, StorageType storage);
        Task<bool> IsBlobAvailableAsync(string fileName, StorageType storage);
        Task<string> UploadFileAsync(Stream content, string fileName, StorageType storage);
        Task DeleteFileAsync(string filePath, StorageType storage);
        Task DeleteFolderAsync(string folderPath, StorageType storage);
        Task CopyFileAsync(string filePathFrom, string filePathTo, StorageType storage);
        Task CopyFileAsync(string filePathFrom, string filePathTo, StorageType storageFrom, StorageType storageTo);
    }
}
