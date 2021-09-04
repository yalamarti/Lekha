using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lekha.Parser
{
    public interface ILoader
    {
        // Load transformed data to the target data store for later retrieval
        Task<StringBuilder> Load(Stream stream);
    }
}
