using FluentAssertions;
using Lekha.Infrastructure;
using Lekha.Uploader.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Lekha.Uploader.Tests.Unit
{
    public class UploadServiceTests
    {
        private readonly ITestOutputHelper output;
        IConfiguration configuration;
        readonly UploaderApplicationContext applicationContext;
        readonly ILogger<UploadService> logger;
        readonly Mock<IBlobClientService<UploadDocument>> mockBlobClientService = new();
        public UploadServiceTests(ITestOutputHelper output)
        {
            applicationContext = new UploaderApplicationContext();
            logger = new NullLogger<UploadService>();
            this.output = output;
        }

        private void SetupConfig(string containerName)
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"Nested:Key1", "NestedValue1"},
                {"Nested:Key2", "NestedValue2"}
            };
            if (string.IsNullOrWhiteSpace(containerName) == false)
            {
                myConfiguration["DocumentContainerName"] = containerName;
            }

            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(500)]
        public async Task ShouldSucceedWhenFilesSpecified(int totalFileCount)
        {
            //
            // Setup
            //
            long count = 0;
            mockBlobClientService.Setup(i => i.Upload(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .Returns(async (string containerName, string blobName, Stream stream) =>
                {
                    var r = new Random();
                    int rInt = r.Next(500, 1500);
                    await Task.Delay(rInt);
                    output.WriteLine($"I was here: {rInt} {DateTime.Now.Ticks}");
                    Interlocked.Increment(ref count);
                });
            SetupConfig("somename");
            var uploadService = new UploadService(applicationContext, mockBlobClientService.Object, configuration, logger);

            //
            // Act
            //
            var list = new List<IFormFile>();

            for (int i = 0; i < totalFileCount; i ++)
            {
                list.Add(new Mock<IFormFile>().Object);
            }
            await uploadService.Upload(list);

            //
            // Assert
            //
            count.Should().Be(totalFileCount);
            mockBlobClientService.Verify(i => i.Upload(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Exactly(totalFileCount));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(500)]
        public async Task ShouldFailWhenFileUploadFails(int totalFileCount)
        {
            //
            // Setup
            //
            long count = 0;
            mockBlobClientService.Setup(i => i.Upload(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .Returns(async (string containerName, string blobName, Stream stream) =>
                {
                    var r = new Random();
                    int rInt = r.Next(500, 1500);
                    await Task.Delay(rInt);
                    output.WriteLine($"I was here: {rInt} {DateTime.Now.Ticks}");
                    Interlocked.Increment(ref count);
                    throw new Exception("This thing failed");
                });
            SetupConfig("somename");
            var uploadService = new UploadService(applicationContext, mockBlobClientService.Object, configuration, logger);

            //
            // Act
            //
            var list = new List<IFormFile>();

            for (int i = 0; i < totalFileCount; i++)
            {
                list.Add(new Mock<IFormFile>().Object);
            }

            var exception = await Record.ExceptionAsync(async () =>
               await uploadService.Upload(list));

            //
            // Assert
            //
            Assert.NotNull(exception);
            Assert.IsType<AggregateException>(exception);
            count.Should().Be(totalFileCount);
            mockBlobClientService.Verify(i => i.Upload(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Exactly(totalFileCount));
        }

        [Fact]
        public async Task ShouldFailWhenNoContainerNameConfigured()
        {
            //
            // Setup
            //
            SetupConfig("");
            var uploadService = new UploadService(applicationContext, mockBlobClientService.Object, configuration, logger);

            //
            // Act
            //
            var exception = await Record.ExceptionAsync(async () =>
               await uploadService.Upload(new List<IFormFile> { new Mock<IFormFile>().Object }));

            //
            // Assert
            //
            mockBlobClientService.Verify(i => i.Upload(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Never);
            Assert.NotNull(exception);
            Assert.IsType<Exception>(exception);
        }

    }
}
