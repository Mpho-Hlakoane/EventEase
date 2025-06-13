using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Threading.Tasks;

namespace EventEase.Services
{
    public class BlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        public BlobService(string connectionString, string containerName)
        {
            _connectionString = connectionString;
            _containerName = containerName;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}

