using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lekha.Uploader.Model
{
    public interface ITransformer
    {
        // Tranform to format usable by Action Executor
        Task<StringBuilder> Transform(Stream stream);
    }
}
