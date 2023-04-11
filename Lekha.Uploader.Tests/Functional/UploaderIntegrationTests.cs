using FluentAssertions;
using Lekha.Uploader.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Lekha.Uploader.Tests.Functional
{
    [Trait("Tests", "Functional")]
    public class UploaderIntegrationTests : IDisposable
    {
        const string ApiEndPoint = "upload/123/Upload";

        private WebApplicationFactory<Startup> _factory;
        public UploaderIntegrationTests()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [Fact]
        public async Task ShouldFailWhenNoFilesSpecified()
        {
            //
            // Setup
            //
            var client = _factory.CreateClient();
            var fileContents = new List<ByteArrayContent>();
            using var form = new MultipartFormDataContent();

            //
            // Act
            //
            var response = await client.PostAsync(ApiEndPoint, form);
            var result = JsonSerializer.Deserialize<UploadResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            //
            // Assert
            //
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Success.Should().Be(false);
            result.UploadId.Should().BeEmpty();

            //
            // Cleanup
            //
            foreach (var f in fileContents) f.Dispose();
        }


        [Theory]
        [InlineData(3, HttpStatusCode.OK)]
        [InlineData(4, HttpStatusCode.OK)]
        [InlineData(128, HttpStatusCode.OK)]
        [InlineData(129, HttpStatusCode.BadRequest)]
        public async Task ShouldCheckFileNameLengthAllowedLimits(int fileNameLength, HttpStatusCode expectedStatusCode)
        {
            //
            // Setup
            //
            var client = _factory.CreateClient();

            var fileContents = new List<ByteArrayContent>();

            using var form = new MultipartFormDataContent();

            for (int i = 0; i < 1; i++)
            {
                string extension = ".txt";
                string testFile = $"{new string('e', (fileNameLength - extension.Length) > 0 ? (fileNameLength - extension.Length) : 0)}{extension}";
                {
                    await File.WriteAllTextAsync(testFile, "test2222222");
                    var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(testFile));
                    fileContents.Add(fileContent);
                    form.Add(fileContent, "documents", Path.GetFileName(testFile));
                }
            }

            //
            // Act
            //
            var response = await client.PostAsync(ApiEndPoint, form);
            var result = JsonSerializer.Deserialize<UploadResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            //
            // Assert
            //
            response.StatusCode.Should().Be(expectedStatusCode);
            result.Success.Should().Be(expectedStatusCode == HttpStatusCode.OK);

            if (expectedStatusCode == HttpStatusCode.OK)
            {
                result.UploadId.Should().NotBeEmpty();
            }

            //
            // Cleanup
            //
            foreach (var f in fileContents) f.Dispose();

            //response.Content.Headers.ContentType?.ToString().Should().Be("application/json; charset=utf-8");
            //json.Should().Be("[{\"fileName\":\"test.pdf\",\"fileSize\":8},{\"fileName\":\"test2.txt\",\"fileSize\":11}]");
        }


        [Theory]
        [InlineData(0, HttpStatusCode.OK)]
        [InlineData(3, HttpStatusCode.OK)]
        [InlineData(4, HttpStatusCode.OK)]
        [InlineData(128, HttpStatusCode.OK)]
        [InlineData(129, HttpStatusCode.OK)]
        [InlineData((10 * 1024 * 1024), HttpStatusCode.OK)]
        [InlineData((10 * 1024 * 1024) + 1, HttpStatusCode.BadRequest)]
        public async Task ShouldCheckFileContentLengthAllowedLimits(int fileSizeLength, HttpStatusCode expectedStatusCode)
        {
            //
            // Setup
            //
            var client = _factory.CreateClient();

            var fileContents = new List<ByteArrayContent>();

            using var form = new MultipartFormDataContent();

            for (int i = 0; i < 1; i++)
            {
                string testFile = "test.txt";
                {
                    await File.WriteAllTextAsync(testFile, new string('e', fileSizeLength));
                    var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(testFile));
                    fileContents.Add(fileContent);
                    form.Add(fileContent, "documents", Path.GetFileName(testFile));
                }
            }

            //
            // Act
            //
            var response = await client.PostAsync(ApiEndPoint, form);
            var responseContentAsString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UploadResponse>(responseContentAsString, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            //
            // Assert
            //
            response.StatusCode.Should().Be(expectedStatusCode);
            result.Success.Should().Be(expectedStatusCode == HttpStatusCode.OK);
            if (expectedStatusCode == HttpStatusCode.OK)
            {
                result.UploadId.Should().NotBeEmpty();
            }

            //
            // Cleanup
            //
            foreach (var f in fileContents) f.Dispose();

            //response.Content.Headers.ContentType?.ToString().Should().Be("application/json; charset=utf-8");
            //json.Should().Be("[{\"fileName\":\"test.pdf\",\"fileSize\":8},{\"fileName\":\"test2.txt\",\"fileSize\":11}]");
        }

        [Theory]
        [InlineData(1, "test", "txt", HttpStatusCode.OK)]
        [InlineData(1, "test", "csv", HttpStatusCode.OK)]
        [InlineData(1, "test", "json", HttpStatusCode.OK)]
        [InlineData(100, "test", "txt", HttpStatusCode.OK)]
        [InlineData(1, "1234567890", "txt", HttpStatusCode.OK)]
        public async Task ShouldUploadOneOrMoreFiles(int fileCount, string fileName, string extension, HttpStatusCode expectedStatusCode)
        {
            //
            // Setup
            //
            var client = _factory.CreateClient();

            var fileContents = new List<ByteArrayContent>();

            using var form = new MultipartFormDataContent();
            for (int i = 0; i < fileCount; i++)
            {
                string testFile = $"{fileName}{i}.{extension}";
                {
                    await File.WriteAllTextAsync(testFile, "test2222222");
                    var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(testFile));
                    fileContents.Add(fileContent);
                    form.Add(fileContent, "documents", Path.GetFileName(testFile));
                }
            }

            //
            // Act
            //
            var response = await client.PostAsync(ApiEndPoint, form);
            var result = JsonSerializer.Deserialize<UploadResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions {  PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            //
            // Assert
            //
            response.StatusCode.Should().Be(expectedStatusCode);
            result.Success.Should().Be(expectedStatusCode == HttpStatusCode.OK);
            result.UploadId.Should().NotBeEmpty();

            //
            // Cleanup
            //
            foreach (var f in fileContents) f.Dispose();

            //response.Content.Headers.ContentType?.ToString().Should().Be("application/json; charset=utf-8");
            //json.Should().Be("[{\"fileName\":\"test.pdf\",\"fileSize\":8},{\"fileName\":\"test2.txt\",\"fileSize\":11}]");
        }

        [Theory]
        [InlineData("jpg")]
        [InlineData("xls")]
        [InlineData("xlsx")]
        [InlineData("docx")]
        public async Task ShouldFailForInvalidExtension(string extension)
        {
            //
            // Setup
            //
            var client = _factory.CreateClient();
            var fileContents = new List<ByteArrayContent>();
            using var form = new MultipartFormDataContent();
            for (int i = 0; i < 1; i++)
            {
                string testFile = $"test{i}.{extension}";
                {
                    await File.WriteAllTextAsync(testFile, "test2222222");
                    var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(testFile));
                    fileContents.Add(fileContent);
                    form.Add(fileContent, "documents", Path.GetFileName(testFile));
                }
            }

            //
            // Act
            //
            var response = await client.PostAsync(ApiEndPoint, form);
            var result = JsonSerializer.Deserialize<UploadResponse>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            //
            // Assert
            //
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            result.Success.Should().Be(false);
            result.UploadId.Should().Be(Guid.Empty);

            //
            // Cleanup
            //
            foreach (var f in fileContents) f.Dispose();
        }

        public void Dispose()
        {
            _factory.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_factory != null)
                {
                    _factory.Dispose();
                    _factory = null;
                }
            }
        }
    }
}
