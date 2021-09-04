using Lekha.Uploader.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lekha.Uploader.Controllers
{

    /// <summary>
    /// controller for upload large file
    /// </summary>
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IUploadService uploadService;
        private readonly IConfiguration configuration;

        public UploadController(IUploadService uploadService, IConfiguration configuration)
        {
            this.uploadService = uploadService;
            this.configuration = configuration;
        }

        // https://codeburst.io/uploading-multiple-files-with-angular-and-net-web-api-7560303d9345

        /// <summary>
        /// Upload one or more documents 
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /111/upload
        ///     {
        ///        "id": 1,
        ///        "name": "Item1",
        ///        "isComplete": true
        ///     }
        ///
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="documents"></param>
        /// <returns>A newly created TodoItem</returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response> 
        [HttpPost("{id:int}/upload")]
        [Produces("application/json")]
        public async Task<ActionResult<UploadResponse>> Upload(int id, [Required] List<IFormFile> documents)
        {
            var retVal = new UploadResponse
            {
                UploadId = Guid.NewGuid()
            };

            if (documents == null || documents.Count == 0)
            {
                retVal.Message = "No file is uploaded.";
                return BadRequest(retVal);
            }
            foreach (var doc in documents)
            {
                string permittedExtensions = configuration.GetValue<string>("PermittedExtensions", ".txt,.csv,.json");

                var ext = Path.GetExtension(doc.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                {
                    retVal.FileName = doc.FileName;
                    retVal.PermittedExtensions = permittedExtensions;
                    retVal.Message = $"No file is uploaded. File with no or invalid extension '{doc.FileName}' specified.  Valid filename extensions: {retVal.PermittedExtensions}";
                    return BadRequest(retVal);
                }
                const short MaxFileNameLength = 256;
                var maxAllowedFileNameLength = configuration.GetValue<long>("MaxFileNameLength", MaxFileNameLength);
                if (doc.FileName.Length > maxAllowedFileNameLength)
                {
                    retVal.FileName = doc.FileName;
                    retVal.Message = $"No file is uploaded. File with name {doc.FileName} exceeds the length limitation.  Maximum allowed name length: {maxAllowedFileNameLength}";
                    return BadRequest(retVal);
                }

                const short MaxFileSize = 256;
                var maxAllowedFileSiz= configuration.GetValue<long>("MaxFileSize", MaxFileSize);
                if (doc.Length > maxAllowedFileSiz)
                {
                    retVal.FileName = doc.FileName;
                    retVal.Message = $"No file is uploaded. File with name {doc.FileName} exceeds the content length limitation.  Maximum allowed file content size: {maxAllowedFileSiz}";
                    return BadRequest(retVal);
                }
            }

            await uploadService.Upload(retVal.UploadId, documents);

            retVal.Success = true;
            return Ok(retVal);
        }
    }
}
