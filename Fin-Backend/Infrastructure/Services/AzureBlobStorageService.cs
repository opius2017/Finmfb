using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System.IO;
using FinTech.Core.Application.Interfaces;

namespace FinTech.Infrastructure.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<AzureBlobStorageService> _logger;

        public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            _blobServiceClient = new BlobServiceClient(connectionString);
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(byte[] fileData, string fileName, string contentType, string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient(fileName);
                
                using (var stream = new MemoryStream(fileData))
                {
                    // Using upload options to set content type could be added here
                    await blobClient.UploadAsync(stream, true);
                }
                
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName} to container {ContainerName}", fileName, containerName);
                throw;
            }
        }

        public async Task<byte[]> DownloadFileAsync(string filePath, string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(filePath); // Assuming filePath is blob name
                
                var response = await blobClient.DownloadAsync();
                
                using (var memoryStream = new MemoryStream())
                {
                    await response.Value.Content.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FilePath} from container {ContainerName}", filePath, containerName);
                throw;
            }
        }

        public async Task DeleteFileAsync(string filePath, string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(filePath); // Assuming filePath is blob name
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FilePath} from container {ContainerName}", filePath, containerName);
                throw;
            }
        }

        public bool IsValidFileType(string fileName, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension)) return false;
            
            foreach (var allowed in allowedExtensions)
            {
                if (allowed.ToLowerInvariant() == extension) return true;
                if (allowed.StartsWith(".") && allowed.ToLowerInvariant() == extension) return true;
                if (!allowed.StartsWith(".") && "." + allowed.ToLowerInvariant() == extension) return true;
            }
            return false;
        }

        public bool IsValidFileSize(long fileSize, long maxFileSize)
        {
            return fileSize <= maxFileSize;
        }
    }
}
