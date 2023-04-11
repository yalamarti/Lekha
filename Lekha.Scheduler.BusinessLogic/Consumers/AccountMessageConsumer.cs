using Lekha.Scheduler.BusinessLogic.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduler.BusinessLogic.Consumers
{
    public class AccountMessageConsumer :
        IConsumer<AccountMessage>
    {
        private readonly TaskGroupScheduler taskGroupScheduler;
        readonly ILogger<AccountMessageConsumer> logger;

        public AccountMessageConsumer(TaskGroupScheduler taskGroupScheduler, ILogger<AccountMessageConsumer> logger)
        {
            this.taskGroupScheduler = taskGroupScheduler;
            this.logger = logger;
        }


        public async Task Consume(ConsumeContext<AccountMessage> context)
        {
            logger.LogInformation("AccountMessageConsumer received : {@account}", context.Message);
            CancellationTokenSource cts = new CancellationTokenSource();
            await taskGroupScheduler.Start(context.Message.AccountId, context.Message.AccountName, cts.Token);
        }
    }
}
