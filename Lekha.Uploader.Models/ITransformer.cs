using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lekha.Uploader.Models
{
    public interface ITransformer
    {
        // Tranform to format usable by Action Executor
        Task<StringBuilder> Transform(Stream stream);
    }
}
