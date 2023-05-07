using System;
using Lekha.GrainInterfaces;
using MassTransit.RabbitMqTransport;
using MassTransit.Transports.Fabric;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace Lekha.TaskGroup.Function
{
    public class TestMessage1
    {
        public TestMessage message { get; set; }
    }
    public class TaskGroupDefinitionFunctions
    {
        private readonly ILogger _logger;

        public TaskGroupDefinitionFunctions(ILoggerFactory loggerFactory)
        {
            
            _logger = loggerFactory.CreateLogger<TaskGroupDefinitionFunctions>();
        }

        [Function("TaskGroupDefinitionFunction")]
        public void RunTaskGroupDefinitionFunction([RabbitMQTrigger("task-group-definition", ConnectionStringSetting = "rabbitMQConnectionAppSetting")] 
        TaskGroupDefinition myQueueItem)
        {
            _logger.LogInformation($"TaskGroupDefinitionFunction: C# Queue trigger function processed: {myQueueItem}");
        }
        [Function("TestFunction")]
        public void RunTestFunction([RabbitMQTrigger("test-queue", ConnectionStringSetting = "rabbitMQConnectionAppSetting")]
        BasicDeliverEventArgs args, TestMessage message)
        {
            _logger.LogInformation($"TestFunction: C# Queue trigger function processed: {message?.id}/{message?.name}");
        }

        //public static class RabbitMQOutput
        //{
        //    [FunctionName("RabbitMQOutput")]
        //    public static async Task Run(
        //    [RabbitMQTrigger("sourceQueue", ConnectionStringSetting = "rabbitMQConnectionAppSetting")] TestClass rabbitMQEvent,
        //    [RabbitMQ(QueueName = "destinationQueue", ConnectionStringSetting = "rabbitMQConnectionAppSetting")] IAsyncCollector<TestClass> outputPocObj,
        //    ILogger log)
        //    {
        //        // send the message
        //        await outputPocObj.AddAsync(rabbitMQEvent);
        //    }
        //}
    }
}
