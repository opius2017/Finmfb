using System;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(byte[] fileData, string fileName, string contentType, string containerName);
        Task<byte[]> DownloadFileAsync(string filePath, string containerName);
        Task DeleteFileAsync(string filePath, string containerName);
        bool IsValidFileType(string fileName, string[] allowedExtensions);
        bool IsValidFileSize(long fileSize, long maxFileSize);
    }
}
