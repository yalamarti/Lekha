using Lekha.Infrastructure;
using Lekha.Uploader.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Lekha.Uploader.Tests.Unit
{
    public class BlobClientServiceTests : IDisposable
    {
        private readonly ITestOutputHelper output;
        IConfiguration configuration;
        UploaderApplicationContext applicationContext;
        ILogger<BlobClientService<UploadDocument>> logger;

        public BlobClientServiceTests(ITestOutputHelper output)
        {
            applicationContext = new UploaderApplicationContext();
            logger = new NullLogger<BlobClientService<UploadDocument>>();
            this.output = output;
        }

        public void Dispose()
        {
        }

        private void SetupConfig(Dictionary<string, string> myConfiguruation)
        {
            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguruation)
                .Build();
        }

        [Fact]
        public async Task ShouldFailWhenNoContainerSpecified()
        {
            //
            // Setup
            //
            SetupConfig(new Dictionary<string, string> { });
            var blobClientService = new BlobClientService<UploadDocument>(applicationContext, configuration, logger);
            var stream = new MemoryStream(223);

            //
            // Act
            //
            var exception = await Record.ExceptionAsync(async () =>
               await blobClientService.Upload(null, "aaa", stream));

            //
            // Assert
            //
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task ShouldFailWhenNoBlobNameSpecified()
        {
            //
            // Setup
            //
            SetupConfig(new Dictionary<string, string> { });
            var blobClientService = new BlobClientService<UploadDocument>(applicationContext, configuration, logger);
            var stream = new MemoryStream(223);

            //
            // Act
            //
            var exception = await Record.ExceptionAsync(async () =>
               await blobClientService.Upload("aaa", null, stream));

            //
            // Assert
            //
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task ShouldFailWhenNoStreamSpecified()
        {
            //
            // Setup
            //
            SetupConfig(new Dictionary<string, string> { });
            var blobClientService = new BlobClientService<UploadDocument>(applicationContext, configuration, logger);

            //
            // Act
            //
            var exception = await Record.ExceptionAsync(async () =>
               await blobClientService.Upload("aaa", "bbb", null));

            //
            // Assert
            //
            Assert.NotNull(exception);
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task ShouldFailWhenNoConnectionStringInConfig()
        {
            //
            // Setup
            //
            SetupConfig(new Dictionary<string, string> { });
            var blobClientService = new BlobClientService<UploadDocument>(applicationContext, configuration, logger);
            var stream = new MemoryStream(223);

            //
            // Act
            //
            var exception = await Record.ExceptionAsync(async () =>
               await blobClientService.Upload("aaa", "bbb", stream));

            //
            // Assert
            //
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
        }
        [Fact]
        public async Task ShouldFailWithNoWhenNoDocumentStorageContainerUriInConfig()
        {
            //
            // Setup
            //
            SetupConfig(new Dictionary<string, string> 
            { 
                { "ManagedIdentityClientId", "aaa" } 
            });
            var blobClientService = new BlobClientService<UploadDocument>(applicationContext, configuration, logger);
            var stream = new MemoryStream(223);

            //
            // Act
            //
            var exception = await Record.ExceptionAsync(async () =>
               await blobClientService.Upload("aaa", "bbb", stream));

            //
            // Assert
            //
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
        }
        [Fact]
        public async Task ShouldFailWithNoConnectionStringWhenGoodConfigBadValues()
        {
            //
            // Setup
            //
            SetupConfig(new Dictionary<string, string>
            {
                { "ManagedIdentityClientId", "aaa" },
                { "DocumentStorageContainerUri", "bbb" }
            });
            var blobClientService = new BlobClientService<UploadDocument>(applicationContext, configuration, logger);
            var stream = new MemoryStream(223);

            //
            // Act
            //
            var exception = await Record.ExceptionAsync(async () =>
               await blobClientService.Upload("aaa", "bbb", stream));

            //
            // Assert
            //
            Assert.NotNull(exception);
            Assert.IsType<UriFormatException>(exception);
        }

        [Fact]
        public async Task ShouldFailWithConnectionStringButadValue()
        {
            //
            // Setup
            //
            SetupConfig(new Dictionary<string, string>
            {
                { "DocumentStorageConnectionString", "aaa" },
            });
            var blobClientService = new BlobClientService<UploadDocument>(applicationContext, configuration, logger);
            var stream = new MemoryStream(223);

            //
            // Act
            //
            var exception = await Record.ExceptionAsync(async () =>
               await blobClientService.Upload("aaa", "bbb", stream));

            //
            // Assert
            //
            Assert.NotNull(exception);
            Assert.IsType<FormatException>(exception);
        }
    }
}
