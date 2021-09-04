using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lekha.Uploader
{
    public interface IUploadService
    {
        Task Upload(Guid uploadId, List<IFormFile> documents);
    }
}
