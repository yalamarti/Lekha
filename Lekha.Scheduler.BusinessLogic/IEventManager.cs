using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduler.BusinessLogic
{
    public interface IEventManager
    {
        Task<bool> Publish<T>(string sourceName, T eventData, CancellationToken cancellationToken) where T : class;
    }
}
