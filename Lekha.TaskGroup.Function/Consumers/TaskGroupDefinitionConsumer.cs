using MassTransit;
using Microsoft.Extensions.Logging;
using Lekha.GrainInterfaces;

namespace Lekha.TaskGroup.Function.Consumers
{
    public struct ConsumerNamespace
    {

    }


    public class TaskGroupDefinitionConsumer :
        IConsumer<TestMessage>
    {
        readonly ILogger<TaskGroupDefinitionConsumer> _logger;

        public TaskGroupDefinitionConsumer(ILogger<TaskGroupDefinitionConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<TestMessage> context)
        {
            _logger.LogInformation("Received Text: {Text}", context.Message?.name);

            return Task.CompletedTask;
        }
    }
}
