using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lekha.Uploader
{
    /// <summary>
    /// Defines Upload service operations
    /// </summary>
    public interface IUploadService
    {
        /// <summary>
        /// Upload specified documents
        /// </summary>
        /// <param name="documents">List of documents from the HttpRequest</param>
        /// <returns>A unique identifier referencing the upload request</returns>
        Task<Guid> Upload(List<IFormFile> documents);
    }
}
