using System;

namespace Lekha.Uploader.Models
{
    /// <summary>
    /// Represents result of an upload request
    /// </summary>
    public class UploadResponse
    {
        /// <summary>
        /// Request processed successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// ID of the upload request
        /// </summary>
        public Guid UploadId { get; set; }

        /// <summary>
        /// Message representing the result of upload operation
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Name of file being uploaded
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Extensions of filenames that are permitted to be uploaded
        /// </summary>
        public string? PermittedExtensions { get; set; }
    }
}
