using Azure.Storage.Blobs;
using Lekha.Infrastructure;
using Lekha.Uploader.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lekha.Uploader
{
    public class UploadService : IUploadService
    {
        private readonly UploaderApplicationContext appContext;
        private readonly IBlobClientService<UploadDocument> blobClientService;
        private readonly IConfiguration configuration;
        private readonly ILogger<UploadService> logger;
        BlobContainerClient blobContainerClient;
        public UploadService(UploaderApplicationContext appContext, IBlobClientService<UploadDocument> blobClientService, IConfiguration configuration, ILogger<UploadService> logger)
        {
            this.appContext = appContext;
            this.blobClientService = blobClientService;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task Upload(Guid uploadId, List<IFormFile> documents)
        {
            List<Task> tasks = new List<Task>();

            logger.LogInformation("{Application}/{Service}/{ServiceLevel2}: Uploading {BlobCountToUpload} to blob container for upload with ID {uploadId}",
                appContext.AppName, appContext.Service, nameof(UploadService), documents.Count, uploadId);

            const string DocumentStorageContainer = "DocumentContainerName";
            string containerName = configuration[DocumentStorageContainer];
            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentNullException($"{DocumentStorageContainer} - Document Container configuration is required!");
            }

            foreach (var doc in documents)
            {
                tasks.Add(Task.Run(async () => 
                {
                    try
                    {
                        logger.LogInformation("{Application}/{Service}/{ServiceLevel2}: Upload to blob container: {BlobToUpload} for upload with ID {uploadId}",
                            appContext.AppName, appContext.Service, nameof(UploadService), doc.FileName, uploadId);

                        using var stream = doc.OpenReadStream();

                        logger.LogInformation("{Application}/{Service}/{ServiceLevel2}: Uploading to blob container: {BlobToUpload} for upload with ID {uploadId}",
                            appContext.AppName, appContext.Service, nameof(UploadService), doc.FileName, uploadId);
                        await blobClientService.Upload(containerName, $"{uploadId}.{doc.FileName}", stream);

                        logger.LogInformation("{Application}/{Service}/{ServiceLevel2}: Uploaded to blob container: {BlobToUpload} for upload with ID {uploadId}",
                            appContext.AppName, appContext.Service, nameof(UploadService), doc.FileName, uploadId);
                    }
                    catch (Exception ex)
                    {
                        throw new ServiceException($"Error processing uploading of the file {doc.FileName}", new { FileName = doc.FileName, UploadId = uploadId }, ex);
                    }
                }));
            }

            logger.LogInformation("{Application}/{Service}/{ServiceLevel2}: Starting tasks for uploading {BlobCountToUpload} to blob container for upload with ID {uploadId}",
                appContext.AppName, appContext.Service, nameof(UploadService), documents.Count, uploadId);

            Task taskReturned = Task.WhenAll(tasks);
            try
            {
                await taskReturned;
            }
            catch
            {
                // taskReturned.Exception will be of type AggregateException
                // https://stackoverflow.com/questions/22240705/how-can-i-catch-an-exception-that-occurs-in-tasks-when-using-task-whenall#43107230
                throw taskReturned.Exception;
            }

            logger.LogInformation("{Application}/{Service}/{ServiceLevel2}: Uploaded {BlobCountToUpload} to blob container for upload with ID {uploadId}",
                appContext.AppName, appContext.Service, nameof(UploadService), documents.Count, uploadId);
        }
    }
}
