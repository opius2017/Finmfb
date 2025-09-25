using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System.IO;
using FinTech.Core.Application.Services;

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

        public async Task<string> UploadFileAsync(byte[] fileContent, string fileName, string containerName)
        {
            try
            {
                // Get a reference to the container
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                
                // Create the container if it doesn't exist
                await containerClient.CreateIfNotExistsAsync();
                
                // Get a reference to the blob
                var blobClient = containerClient.GetBlobClient(fileName);
                
                // Upload the file
                using (var stream = new MemoryStream(fileContent))
                {
                    await blobClient.UploadAsync(stream, true);
                }
                
                // Return the blob URL
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName} to container {ContainerName}", fileName, containerName);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Parse the URL to get container name and blob name
                var uri = new Uri(fileUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                
                if (pathSegments.Length < 3)
                {
                    throw new ArgumentException("Invalid file URL format");
                }
                
                var containerName = pathSegments[1];
                var blobName = string.Join('/', pathSegments.Skip(2));
                
                // Get a reference to the container
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                
                // Get a reference to the blob
                var blobClient = containerClient.GetBlobClient(blobName);
                
                // Delete the blob
                var response = await blobClient.DeleteIfExistsAsync();
                
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from URL {FileUrl}", fileUrl);
                throw;
            }
        }

        public async Task<byte[]> DownloadFileAsync(string fileUrl)
        {
            try
            {
                // Parse the URL to get container name and blob name
                var uri = new Uri(fileUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                
                if (pathSegments.Length < 3)
                {
                    throw new ArgumentException("Invalid file URL format");
                }
                
                var containerName = pathSegments[1];
                var blobName = string.Join('/', pathSegments.Skip(2));
                
                // Get a reference to the container
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                
                // Get a reference to the blob
                var blobClient = containerClient.GetBlobClient(blobName);
                
                // Download the blob
                var response = await blobClient.DownloadAsync();
                
                using (var memoryStream = new MemoryStream())
                {
                    await response.Value.Content.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file from URL {FileUrl}", fileUrl);
                throw;
            }
        }
    }
}
