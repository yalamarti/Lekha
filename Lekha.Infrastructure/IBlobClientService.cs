using System.IO;
using System.Threading.Tasks;

namespace Lekha.Infrastructure
{
    public interface IBlobClientService<T>
    {
        Task Upload(string containerName, string blobName, Stream stream);
    }
}
