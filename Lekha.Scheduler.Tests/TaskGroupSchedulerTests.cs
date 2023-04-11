using FluentAssertions;
using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using Lekha.Scheduler.BusinessLogic;
using Lekha.Scheduler.BusinessLogic.Messages;
using Lekha.Scheduler.BusinessLogic.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Lekha.Scheduler.Tests
{
    public class TaskGroupSchedulerTests
    {
        IConfiguration configuration;

        private readonly ILogger<TaskGroupScheduler> logger;
        readonly Mock<IScheduleDomain> mockScheduleDomain = new();
        readonly Mock<IEventManager> mockEventManager = new();
        private IReadOnlyPolicyRegistry<string> mockPolicyRegistry;
        List<Account> accounts = new List<Account>
        {
            new Account
            {
                Id = Guid.NewGuid().ToString(),
                Name = "1",
                TaskGroups = new List<TaskGroupDefinition>
                {
                    new TaskGroupDefinition
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "1"
                    },
                    new TaskGroupDefinition
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "2"
                    },
                    new TaskGroupDefinition
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "3"
                    },
                    new TaskGroupDefinition
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "4"
                    },
                }
            }
        };

        public TaskGroupSchedulerTests(ITestOutputHelper testOutputHelper)
        {
            var loggerFactory = LoggerFactory.Create(l =>
            {
                l.AddProvider(new XunitLoggerProvider(testOutputHelper));
            });
            logger = loggerFactory.CreateLogger<TaskGroupScheduler>();
        }
        private void Setup(int publishRetryCount, int dataRetrievalRetryCount, int resourceGroupRetrievalPageSize)
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {$"{ConfigurationNames.DataRetrievalRetryCount}", dataRetrievalRetryCount.ToString()},
                {$"{ConfigurationNames.PublishRetryRetryCount}", publishRetryCount.ToString()},
                {$"{ConfigurationNames.TaskGroupRetrievalPageSize}", resourceGroupRetrievalPageSize.ToString()}
            };

            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            mockPolicyRegistry = Configuration.GetPollyPolicyRegistry(configuration);
        }
        const string ExceptionMessage = "Some error";

        private void SetupTaskGroup(bool fail)
        {
            mockScheduleDomain.Setup(i => i.GetTaskGroupDefinitionsAsync(It.IsAny<string>(), It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>())).
                ReturnsAsync((string accountId, PaginationRequest paginationRequest, CancellationToken cancellationToken) =>
                {
                    if (fail)
                    {
                        throw new Exception(ExceptionMessage);
                    }
                    if (paginationRequest.ContinuationToken == null)
                    {
                        return new PaginationResult<TaskGroupDefinition>
                        {
                            Items = accounts.First().TaskGroups.Take(paginationRequest.PageSize).ToList(),
                            ContinuationToken = paginationRequest.PageSize.ToString(),
                            HasMoreItems = accounts.First().TaskGroups.Count > paginationRequest.PageSize
                        };
                    }
                    var startIndex = int.Parse(paginationRequest.ContinuationToken);
                    return new PaginationResult<TaskGroupDefinition>
                    {
                        Items = accounts.First().TaskGroups.Skip(startIndex).Take(paginationRequest.PageSize).ToList(),
                        ContinuationToken = (startIndex + paginationRequest.PageSize).ToString(),
                        HasMoreItems = accounts.First().TaskGroups.Count > (startIndex + paginationRequest.PageSize) 
                    };
                });

        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(1000)]
        public async Task TaskGroupLevelScheduleShouldStartSuccessfullyWhenNoError1(int paginationRequestageSize)
        {
            //
            // Setup
            //
            const int DataRetrievalRetryCount = 0;
            const int PublishCallRetryCount = 0;
            Setup(PublishCallRetryCount, DataRetrievalRetryCount, paginationRequestageSize);

            SetupTaskGroup(false);

            var scheduler = new TaskGroupScheduler(mockScheduleDomain.Object,
                         mockEventManager.Object,
                         configuration,
                         mockPolicyRegistry,
                         logger);

            //
            // Act
            //
            var cancellationToken = new CancellationToken();
            await scheduler.Start(accounts.First().Id, accounts.First().Name, cancellationToken);

            //
            // Verify
            //
            var factor = (accounts.First().TaskGroups.Count / paginationRequestageSize) < 1 ? 1 : (accounts.First().TaskGroups.Count / paginationRequestageSize);
            mockScheduleDomain.Verify(i => i.GetTaskGroupDefinitionsAsync(It.IsAny<string>(), It.IsAny<PaginationRequest>(), cancellationToken), Times.Exactly(factor * (DataRetrievalRetryCount + 1)));
            mockEventManager.Verify(i => i.Publish(EventSources.ScheduleTaskGroup,
                It.IsAny<TaskGroupMessage>(), cancellationToken), Times.Exactly((PublishCallRetryCount + 1) * accounts.First().TaskGroups.Count));
            foreach (var item in accounts.First().TaskGroups)
            {
                mockEventManager.Verify(i => i.Publish(EventSources.ScheduleTaskGroup,
                    It.Is<TaskGroupMessage>(i => i.AccountId == accounts.First().Id 
                        && i.AccountName == accounts.First().Name
                        && i.TaskGroupId == item.Id
                        && i.TaskGroupName == item.Name), cancellationToken), Times.Exactly(1));
            }
        }


        [Fact]
        public async Task TaskGroupLevelScheduleShouldFailToStartWhenFailedToRetrieveTaskGroups()
        {
            //
            // Setup
            //
            const int DataRetrievalRetryCount = 2;
            const int PublishCallRetryCount = 2;
            const int paginationRequestageSize = 4;
            const string ExceptionMessage = "Some error";
            Setup(PublishCallRetryCount, DataRetrievalRetryCount, paginationRequestageSize);

            SetupTaskGroup(true);

            var scheduler = new TaskGroupScheduler(mockScheduleDomain.Object,
                         mockEventManager.Object,
                         configuration,
                         mockPolicyRegistry,
                         logger);

            //
            // Act
            //
            var cancellationToken = new CancellationToken();

            try
            {
                await scheduler.Start(accounts.First().Id, accounts.First().Name, cancellationToken);
            }
            catch (Exception ex)
            {
                ex.InnerException.Message.Should().Be(ExceptionMessage);
            }
            //
            // Verify
            //
            mockScheduleDomain.Verify(i => i.GetTaskGroupDefinitionsAsync(It.IsAny<string>(), It.IsAny<PaginationRequest>(), cancellationToken), Times.Exactly((DataRetrievalRetryCount + 1)));
            mockEventManager.Verify(i => i.Publish(EventSources.ScheduleTaskGroup,
                It.IsAny<TaskGroupMessage>(), cancellationToken), Times.Never);
        }

        [Fact]
        public async Task TaskGroupLevelScheduleShouldFailToStartWhenFailedToPublishTaskGroupEvents()
        {
            //
            // Setup
            //
            const int DataRetrievalRetryCount = 0;
            const int PublishCallRetryCount = 2;
            const int paginationRequestageSize = 4;
            var cancellationToken = new CancellationToken();
            Setup(PublishCallRetryCount, DataRetrievalRetryCount, paginationRequestageSize);

            SetupTaskGroup(false);

            mockEventManager.Setup(i => i.Publish(EventSources.ScheduleTaskGroup, It.IsAny<TaskGroupMessage>(), cancellationToken))
                .ReturnsAsync((string sourceName, TaskGroupMessage eventData, CancellationToken cancellationToken) =>
                {
                    throw new Exception(ExceptionMessage);
                });
            var scheduler = new TaskGroupScheduler(mockScheduleDomain.Object,
                         mockEventManager.Object,
                         configuration,
                         mockPolicyRegistry,
                         logger);

            //
            // Act
            //

            try
            {
                await scheduler.Start(accounts.First().Id, accounts.First().Name, cancellationToken);
            }
            catch (Exception ex)
            {
                
                ex.Message.Should().StartWith($"Attempted {accounts.First().TaskGroups.Count} - but failed to start triggering of scheduling for {accounts.First().TaskGroups.Count} task groups");
            }
            //
            // Verify
            //
            var factor = (accounts.First().TaskGroups.Count / paginationRequestageSize) < 1 ? 1 : (accounts.First().TaskGroups.Count / paginationRequestageSize);
            mockScheduleDomain.Verify(i => i.GetTaskGroupDefinitionsAsync(It.IsAny<string>(), It.IsAny<PaginationRequest>(), cancellationToken), Times.Exactly(factor * (DataRetrievalRetryCount + 1)));
            mockEventManager.Verify(i => i.Publish(EventSources.ScheduleTaskGroup,
                It.IsAny<TaskGroupMessage>(), cancellationToken), Times.Exactly(accounts.First().TaskGroups.Count + (accounts.First().TaskGroups.Count * PublishCallRetryCount)));
        }

        [Fact]
        public async Task TaskGroupLevelScheduleShouldStartSomeAndFailSomeWhenFailedToPublishOrganizationEventsForSome()
        {
            //
            // Setup
            //
            const int DataRetrievalRetryCount = 0;
            const int PublishCallRetryCount = 2;
            const int paginationRequestageSize = 4;
            const string ExceptionMessage = "Some error";
            var cancellationToken = new CancellationToken();
            Setup(PublishCallRetryCount, DataRetrievalRetryCount, paginationRequestageSize);

            SetupTaskGroup(false);

            var toFaileOrganizationNames = new List<string> { "1", "3" };
            mockEventManager.Setup(i => i.Publish(EventSources.ScheduleTaskGroup, It.IsAny<TaskGroupMessage>(), cancellationToken))
                .ReturnsAsync((Func<string, TaskGroupMessage, CancellationToken, bool>)((string sourceName, TaskGroupMessage eventData, CancellationToken cancellationToken) =>
                {
                    if (toFaileOrganizationNames.Contains((string)eventData.TaskGroupName))
                    {
                        throw new Exception(ExceptionMessage);
                    }
                    return true;
                }));
            var scheduler = new TaskGroupScheduler(mockScheduleDomain.Object,
                         mockEventManager.Object,
                         configuration,
                         mockPolicyRegistry,
                         logger);

            //
            // Act
            //
            try
            {
                await scheduler.Start(accounts.First().Id, accounts.First().Name, cancellationToken);
            }
            catch (Exception ex)
            {
                
                ex.Message.Should().StartWith($"Attempted { accounts.First().TaskGroups.Count} - but failed to start triggering of scheduling for { toFaileOrganizationNames.Count} task groups");
            }
            //
            // Verify
            //
            var factor = (accounts.First().TaskGroups.Count / paginationRequestageSize) < 1 ? 1 : (accounts.First().TaskGroups.Count / paginationRequestageSize);
            mockScheduleDomain.Verify(i => i.GetTaskGroupDefinitionsAsync(It.IsAny<string>(), It.IsAny<PaginationRequest>(), cancellationToken), Times.Exactly(factor * (DataRetrievalRetryCount + 1)));
            mockEventManager.Verify(i => i.Publish(EventSources.ScheduleTaskGroup,
                It.IsAny<TaskGroupMessage>(), cancellationToken), Times.Exactly(accounts.First().TaskGroups.Count + ((accounts.First().TaskGroups.Count - toFaileOrganizationNames.Count) * PublishCallRetryCount)));
        }
    }
}
