using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lekha.Uploader.Model
{
    public interface ILoader
    {
        // Load transformed data to the target data store for later retrieval
        Task<StringBuilder> Load(Stream stream);
    }
}
