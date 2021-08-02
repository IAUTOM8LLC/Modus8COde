using IAutoM8.Global.Enums;
using IAutoM8.Service.CommonService.Interfaces;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.Service.CommonService
{
    public class StorageService : IStorageService
    {
        private readonly CloudBlobClient _blobClient;

        public StorageService (CloudBlobClient blobClient)
        {
            _blobClient = blobClient;
        }

        public async Task CopyFileAsync(string filePathFrom, string filePathTo, StorageType storage)
        {
            await CopyFileAsync(filePathFrom, filePathTo, storage, storage);
        }

        public async Task CopyFileAsync(string filePathFrom, string filePathTo, StorageType storageFrom, StorageType storageTo)
        {
            CloudBlockBlob sourceBlob = GetConteiner(storageFrom).GetBlockBlobReference(filePathFrom);
            CloudBlockBlob targetBlob = GetConteiner(storageTo).GetBlockBlobReference(filePathTo);
            await targetBlob.StartCopyAsync(sourceBlob);
        }

        public async Task DeleteFileAsync(string filePath, StorageType storage)
        {
            await GetConteiner(storage)
                .GetBlockBlobReference(filePath).DeleteIfExistsAsync();
        }

        public async Task DeleteFolderAsync(string folderPath, StorageType storage)
        {
            foreach (var file in (await GetConteiner(storage)
                .GetDirectoryReference(folderPath)
                .ListBlobsSegmentedAsync(new BlobContinuationToken())).Results
                .Where(file => file is CloudBlob))
            {
                await ((CloudBlob)file).DeleteIfExistsAsync();
            }
        }

        public string GetFileUri(string fileName, StorageType storage)
        {
            return GetConteiner(storage).GetBlobReference(fileName).Uri.AbsoluteUri;
        }

        public string GetProfileImageUri(string fileName)
        {
            var blob = GetConteiner(StorageType.ProfileImage).GetBlobReference(fileName);

            if (!blob.ExistsAsync().Result)
            {
                return string.Empty;
            }

            return blob.Uri.AbsoluteUri;
        }

        public async Task<string> UploadFileAsync(Stream content, string fileName, StorageType storage)
        {
            if (storage == StorageType.Description)
            {
                fileName = fileName.Replace(Path.GetFileNameWithoutExtension(fileName), DateTime.Now.Ticks.ToString());
            }
            var blob = GetConteiner(storage).GetBlockBlobReference(fileName);
            await blob.UploadFromStreamAsync(content);
            return blob.Uri.AbsoluteUri;
        }

        public async Task<bool> IsBlobAvailableAsync(string fileName, StorageType storage)
        {
            return await GetConteiner(storage).GetBlockBlobReference(fileName).ExistsAsync();
        }

        private CloudBlobContainer GetConteiner(StorageType storage)
        {
            switch (storage)
            {
                case StorageType.Formula:
                    return _blobClient.GetContainerReference("formula");
                case StorageType.FormulaTask:
                    return _blobClient.GetContainerReference("formula-task");
                case StorageType.Project:
                    return _blobClient.GetContainerReference("project");
                case StorageType.Temp:
                    return _blobClient.GetContainerReference("temp");
                case StorageType.Description:
                    return _blobClient.GetContainerReference("description");
                case StorageType.ProfileImage:
                    return _blobClient.GetContainerReference("profile-image");
                default:
                    return _blobClient.GetContainerReference("project-task");
            }
        }
    }
}
