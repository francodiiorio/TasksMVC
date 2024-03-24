using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using TareasMVC.Models;

namespace TareasMVC.Services
{
    public class StorageFilesAzure: IFilesStorage
    {
        private string connectionString;
        public StorageFilesAzure(IConfiguration configuration) 
        {
            connectionString = configuration.GetConnectionString("AzureStorage");
        }

        public async Task Delete(string path, string container)
        {
            if(string.IsNullOrEmpty(path))
            {
                return;
            }

            var client = new BlobContainerClient(connectionString, container);
            await client.CreateIfNotExistsAsync();
            var fileName = Path.GetFileName(path);
            var blob = client.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<StorageFileResult[]> Storage(string container, IEnumerable<IFormFile> files)
        {
            var client = new BlobContainerClient(connectionString, container);
            await client.CreateIfNotExistsAsync();
            client.SetAccessPolicy(PublicAccessType.Blob);

            var tasks = files.Select(async file =>
            {
                var nameOriginalFile = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var blob = client.GetBlobClient(fileName);
                var blobHttpHeaders = new BlobHttpHeaders();
                blobHttpHeaders.ContentType = file.ContentType;
                await blob.UploadAsync(file.OpenReadStream(), blobHttpHeaders);
                return new StorageFileResult
                {
                    Url = blob.Uri.ToString(),
                    Title = nameOriginalFile
                };
            });
            var result = await Task.WhenAll(tasks);
            return result;
        }
    }
}
