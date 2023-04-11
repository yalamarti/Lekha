using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduler.BusinessLogic
{
    public class EventManager : IEventManager
    {
        private readonly IBus bus;
        private readonly ILogger<EventManager> logger;

        public EventManager(IBus bus, ILogger<EventManager> logger)
        {
            this.bus = bus;
            this.logger = logger;
        }
        public async Task<bool> Publish<T>(string sourceName, T eventData, CancellationToken cancellationToken) where T : class
        {
            await bus.Publish(eventData, cancellationToken);
            return true;
        }
    }
}
