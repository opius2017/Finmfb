using System.Threading.Tasks;

namespace FinTech.Application.Common.Interfaces
{
    public interface IAzureBlobStorageService
    {
        Task<string> UploadFileAsync(string blobName, byte[] content);
        Task<bool> DeleteFileAsync(string blobUri);
        Task<byte[]> DownloadFileAsync(string blobUri);
        Task<bool> BlobExistsAsync(string blobName);
    }
}