using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lekha.Parser
{
    public interface IExtractor
    {
        // Apply filter
        // Remove duplicates
        Task<StringBuilder> Extract(Stream stream);
    }
}
