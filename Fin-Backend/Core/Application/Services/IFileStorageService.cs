using System.Threading.Tasks;

namespace FinTech.Core.Application.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(byte[] fileContent, string fileName, string containerName);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<byte[] > DownloadFileAsync(string fileUrl);
    }
}
