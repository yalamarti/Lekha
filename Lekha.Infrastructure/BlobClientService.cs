using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Lekha.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Lekha.Infrastructure
{
    public class BlobClientService<T>: IBlobClientService<T>
    {
        private readonly ApplicationContext appContext;
        private readonly IConfiguration configuration;
        private readonly ILogger<BlobClientService<T>> logger;
        private BlobContainerClient blobContainerClient;
        public BlobClientService(ApplicationContext appContext, IConfiguration configuration, ILogger<BlobClientService<T>> logger)
        {
            this.appContext = appContext;
            this.configuration = configuration;
            this.logger = logger;
        }

        private async Task SetupClient(string containerName)
        {
            if (blobContainerClient == null)
            {
                var connectionString = configuration["DocumentStorageConnectionString"];
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    // has to be managed identity

                    const string ManagedIdentityClientId = "ManagedIdentityClientId";
                    string userAssignedClientId = configuration[ManagedIdentityClientId];
                    if (string.IsNullOrWhiteSpace(userAssignedClientId))
                    {
                        throw new Exception($"{ManagedIdentityClientId} - Connection string or Managed Identity configuration required!");
                    }

                    const string DocumentStorageContainerUri = "DocumentStorageContainerUri";
                    string uri = configuration[DocumentStorageContainerUri];
                    if (string.IsNullOrWhiteSpace(uri))
                    {
                        throw new Exception($"{DocumentStorageContainerUri} - Document Container URI configuration required!");
                    }

                    logger.LogInformation("{Application}/{Service}/{ServiceLevel2}: Using Managed Identity configuration",
                        appContext.AppName, appContext.Service, nameof(BlobClientService<T>));

                    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    {
                        ManagedIdentityClientId = userAssignedClientId
                    });

                    blobContainerClient = new BlobContainerClient(new Uri(new Uri(uri), containerName), credential);
                }
                else
                {
                    blobContainerClient = new BlobContainerClient(connectionString, containerName);
                }
                await blobContainerClient.CreateIfNotExistsAsync();
            }
        }

        public async Task Upload(string containerName, string blobName, Stream stream)
        {
            //const string DocumentStorageContainer = "DocumentContainerName";
            //string containerName = configuration[DocumentStorageContainer];
            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentNullException($"{nameof(containerName)} is required!");
            }
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentNullException($"{nameof(blobName)} is required!");
            }
            if (stream == null)
            {
                throw new ArgumentNullException($"{nameof(stream)} is required!");
            }

            await SetupClient(containerName);

            BlobUploadOptions options = new BlobUploadOptions
            {
                TransferOptions = new StorageTransferOptions
                {
                    // Set the maximum number of parallel transfer workers
                    MaximumConcurrency = 2,

                    // Set the initial transfer length to 8 MiB
                    InitialTransferSize = 8 * 1024 * 1024,

                    // Set the maximum length of a transfer to 4 MiB
                    MaximumTransferSize = 4 * 1024 * 1024
                }
            };
            
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(stream, options);
        }
    }
}
