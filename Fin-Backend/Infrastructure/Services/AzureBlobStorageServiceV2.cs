using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using FinTech.Core.Application.Common.Interfaces;

namespace FinTech.Infrastructure.Services
{
    public class AzureBlobStorageServiceV2 : IAzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly ILogger<AzureBlobStorageServiceV2> _logger;

        public AzureBlobStorageServiceV2(IConfiguration configuration, ILogger<AzureBlobStorageServiceV2> logger)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            _containerName = configuration["AzureBlobStorage:ClientPortalContainer"] ?? "client-portal";
            _blobServiceClient = new BlobServiceClient(connectionString);
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(string blobName, byte[] content)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                
                // Create the container if it doesn't exist
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
                
                var blobClient = containerClient.GetBlobClient(blobName);
                
                using (var stream = new MemoryStream(content))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
                
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading blob {BlobName}", blobName);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string blobUri)
        {
            try
            {
                var blobUriObj = new Uri(blobUri);
                var blobName = Path.GetFileName(blobUriObj.LocalPath);
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blob at URI {BlobUri}", blobUri);
                throw;
            }
        }

        public async Task<byte[]> DownloadFileAsync(string blobUri)
        {
            try
            {
                var blobUriObj = new Uri(blobUri);
                var blobName = Path.GetFileName(blobUriObj.LocalPath);
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                var response = await blobClient.DownloadAsync();
                
                using (var memoryStream = new MemoryStream())
                {
                    await response.Value.Content.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading blob at URI {BlobUri}", blobUri);
                throw;
            }
        }

        public async Task<bool> BlobExistsAsync(string blobName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                var response = await blobClient.ExistsAsync();
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if blob {BlobName} exists", blobName);
                throw;
            }
        }
    }
}
