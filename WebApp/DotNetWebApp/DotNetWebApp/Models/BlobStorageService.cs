using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DotNetWebApp.Models
{
    public class BlobStorageService
    {
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=storagedotnet;AccountKey=kwPpHY67tCbPQqEGv8hgZ4cLOoRi1ylveBcdX+rFcwKUwrRu4XHkftS7zUleETvvPCH/HO/K46Nn+AStgOSzug==;EndpointSuffix=core.windows.net";
        private readonly string _containerName = "photos";

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            // Tworzenie klienta połączenia z Azure Blob Storage
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            // Generowanie unikalnej nazwy pliku (można użyć np. GUID)
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var blobClient = blobContainerClient.GetBlobClient(fileName);

            // Wgrywanie pliku do Blob Storage
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();  // Zwrócenie URL-a do wgranego pliku
        }

		public async Task<string> UploadFileUserDataAsync(IFormFile file, string userID)
		{
			// Tworzenie klienta połączenia z Azure Blob Storage
			var blobServiceClient = new BlobServiceClient(_connectionString);
			var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

			// Generowanie unikalnej nazwy pliku (można użyć np. GUID)
			var fileName = $"{Guid.NewGuid()}_{file.FileName}";
			var blobClient = blobContainerClient.GetBlobClient(fileName);

			// Wgrywanie pliku do Blob Storage
			using (var stream = file.OpenReadStream())
			{
				await blobClient.UploadAsync(stream, overwrite: true);
			}

			return blobClient.Uri.ToString();  // Zwrócenie URL-a do wgranego pliku
		}
	}
}
