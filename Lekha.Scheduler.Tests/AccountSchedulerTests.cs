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
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Lekha.Scheduler.Tests
{
    public class AccountSchedulerTests
    {
        IConfiguration configuration;

        private readonly ILogger<AccountScheduler> logger;
        readonly Mock<IScheduleDomain> mockScheduleDomain = new Mock<IScheduleDomain>();
        readonly Mock<IEventManager> mockEventManager = new Mock<IEventManager>();
        private IReadOnlyPolicyRegistry<string> mockPolicyRegistry;
        List<Account> accounts = new List<Account>
            {
                new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "1"
                },
                new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "2"
                },
                new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "3"
                },
                new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "4"
                },
            };

        public AccountSchedulerTests(ITestOutputHelper testOutputHelper)
        {
            var loggerFactory = LoggerFactory.Create(l =>
            {
                l.AddProvider(new XunitLoggerProvider(testOutputHelper));
            });
            logger = loggerFactory.CreateLogger<AccountScheduler>();
        }

        private void Setup(int publishRetryCount, int dataRetrievalRetryCount, int accountRetrievalPageSize)
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {$"{ConfigurationNames.DataRetrievalRetryCount}", dataRetrievalRetryCount.ToString()},
                {$"{ConfigurationNames.PublishRetryRetryCount}", publishRetryCount.ToString()},
                {$"{ConfigurationNames.AccountRetrievalPageSize}", accountRetrievalPageSize.ToString()}
            };

            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            mockPolicyRegistry = Configuration.GetPollyPolicyRegistry(configuration);
        }
        const string ExceptionMessage = "Some error";

        private void SetupAccount(bool fail)
        {

            mockScheduleDomain.Setup(i => i.GetAccountsAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>())).
                ReturnsAsync((PaginationRequest paginationRequest, CancellationToken cancellationToken) =>
                {
                    if (fail)
                    {
                        throw new Exception(ExceptionMessage);
                    }
                    if (paginationRequest.ContinuationToken == null)
                    {
                        return new PaginationResult<Account>
                        {
                            Items = accounts.Take(paginationRequest.PageSize).ToList(),
                            ContinuationToken = paginationRequest.PageSize.ToString(),
                            HasMoreItems = accounts.Count > paginationRequest.PageSize
                        };
                    }
                    var startIndex = int.Parse(paginationRequest.ContinuationToken);
                    return new PaginationResult<Account>
                    {
                        Items = accounts.Skip(startIndex).Take(paginationRequest.PageSize).ToList(),
                        ContinuationToken = (startIndex + paginationRequest.PageSize).ToString(),
                        HasMoreItems = accounts.Count > (startIndex + paginationRequest.PageSize) 
                    };
                });

        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(1000)]
        public async Task AccountLevelScheduleShouldStartSuccessfullyWhenNoError1(int paginationRequestageSize)
        {
            //
            // Setup
            //
            const int AccountCallRetryCount = 0;
            const int PublishCallRetryCount = 0;
            Setup(PublishCallRetryCount, AccountCallRetryCount, paginationRequestageSize);

            SetupAccount(false);

            var scheduler = new AccountScheduler(mockScheduleDomain.Object,
                         mockEventManager.Object,
                         configuration,
                         mockPolicyRegistry,
                         logger);

            //
            // Act
            //
            var cancellationToken = new CancellationToken();
            await scheduler.Start(cancellationToken);

            //
            // Verify
            //
            var factor = (accounts.Count / paginationRequestageSize) < 1 ? 1 : (accounts.Count / paginationRequestageSize);
            mockScheduleDomain.Verify(i => i.GetAccountsAsync(It.IsAny<PaginationRequest>(), cancellationToken), Times.Exactly(factor * (AccountCallRetryCount + 1)));
            mockEventManager.Verify(i => i.Publish(EventSources.ScheduleAccount,
                It.IsAny<AccountMessage>(), cancellationToken), Times.Exactly((PublishCallRetryCount + 1) * accounts.Count));
            foreach (var item in accounts)
            {
                mockEventManager.Verify(i => i.Publish(EventSources.ScheduleAccount,
                    It.Is<AccountMessage>(i => i.AccountId == item.Id && i.AccountName == item.Name), cancellationToken), Times.Exactly(1));
            }
        }


        [Fact]
        public async Task AccountLevelScheduleShouldFailToStartWhenFailedToRetrieveAccounts()
        {
            //
            // Setup
            //
            const int AccountCallRetryCount = 2;
            const int PublishCallRetryCount = 2;
            const int paginationRequestageSize = 4;
            const string ExceptionMessage = "Some error";
            Setup(PublishCallRetryCount, AccountCallRetryCount, paginationRequestageSize);

            SetupAccount(true);

            var scheduler = new AccountScheduler(mockScheduleDomain.Object,
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
                await scheduler.Start(cancellationToken);
            }
            catch (Exception ex)
            {
                ex.InnerException.Message.Should().Be(ExceptionMessage);
            }
            //
            // Verify
            //
            mockScheduleDomain.Verify(i => i.GetAccountsAsync(It.IsAny<PaginationRequest>(), cancellationToken), Times.Exactly((AccountCallRetryCount + 1)));
            mockEventManager.Verify(i => i.Publish(EventSources.ScheduleAccount,
                It.IsAny<AccountMessage>(), cancellationToken), Times.Never);
        }

        [Fact]
        public async Task AccountLevelScheduleShouldFailToStartWhenFailedToPublishOrganizationEvents()
        {
            //
            // Setup
            //
            const int AccountCallRetryCount = 0;
            const int PublishCallRetryCount = 2;
            const int paginationRequestageSize = 4;
            var cancellationToken = new CancellationToken();
            Setup(PublishCallRetryCount, AccountCallRetryCount, paginationRequestageSize);

            SetupAccount(false);

            mockEventManager.Setup(i => i.Publish(EventSources.ScheduleAccount, It.IsAny<AccountMessage>(), cancellationToken))
                .ReturnsAsync((string sourceName, AccountMessage eventData, CancellationToken cancellationToken) =>
                {
                    throw new Exception(ExceptionMessage);
                });
            var scheduler = new AccountScheduler(mockScheduleDomain.Object,
                         mockEventManager.Object,
                         configuration,
                         mockPolicyRegistry,
                         logger);

            //
            // Act
            //

            try
            {
                await scheduler.Start(cancellationToken);
            }
            catch (Exception ex)
            {
                ex.Message.Should().StartWith($"Attempted {accounts.Count} - but failed to start triggering of scheduling for {accounts.Count} accounts");
            }
            //
            // Verify
            //
            mockScheduleDomain.Verify(i => i.GetAccountsAsync(It.IsAny<PaginationRequest>(), cancellationToken), Times.Exactly((accounts.Count / paginationRequestageSize) * (AccountCallRetryCount + 1)));
            mockEventManager.Verify(i => i.Publish(EventSources.ScheduleAccount,
                It.IsAny<AccountMessage>(), cancellationToken), Times.Exactly(accounts.Count + (accounts.Count * PublishCallRetryCount)));
        }

        [Fact]
        public async Task AccountLevelScheduleShouldStartSomeAndFailSomeWhenFailedToPublishOrganizationEventsForSome()
        {
            //
            // Setup
            //
            const int AccountCallRetryCount = 0;
            const int PublishCallRetryCount = 2;
            const int paginationRequestageSize = 4;
            const string ExceptionMessage = "Some error";
            var cancellationToken = new CancellationToken();
            Setup(PublishCallRetryCount, AccountCallRetryCount, paginationRequestageSize);

            SetupAccount(false);

            var toFaileOrganizationNames = new List<string> { "1", "3" };
            mockEventManager.Setup(i => i.Publish(EventSources.ScheduleAccount, It.IsAny<AccountMessage>(), cancellationToken))
                .ReturnsAsync((Func<string, AccountMessage, CancellationToken, bool>)((string sourceName, AccountMessage eventData, CancellationToken cancellationToken) =>
                {
                    if (toFaileOrganizationNames.Contains((string)eventData.AccountName))
                    {
                        throw new Exception(ExceptionMessage);
                    }
                    return true;
                }));
            var scheduler = new AccountScheduler(mockScheduleDomain.Object,
                         mockEventManager.Object,
                         configuration,
                         mockPolicyRegistry,
                         logger);

            //
            // Act
            //

            try
            {
                await scheduler.Start(cancellationToken);
            }
            catch (Exception ex)
            {
                
                ex.Message.Should().StartWith($"Attempted {accounts.Count} - but failed to start triggering of scheduling for {toFaileOrganizationNames.Count} accounts");
            }
            //
            // Verify
            //
            mockScheduleDomain.Verify(i => i.GetAccountsAsync(It.IsAny<PaginationRequest>(), cancellationToken), Times.Exactly((accounts.Count / paginationRequestageSize) * (AccountCallRetryCount + 1)));
            mockEventManager.Verify(i => i.Publish(EventSources.ScheduleAccount,
                It.IsAny<AccountMessage>(), cancellationToken), Times.Exactly(accounts.Count + ((accounts.Count - toFaileOrganizationNames.Count) * PublishCallRetryCount)));
        }

        [Fact]
        public void TestQuartz()
        {
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger3", "group1")
                .WithCronSchedule("0 42 10 ? * WED", x => x
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time")))
                .Build();
            ITrigger trigger2 = TriggerBuilder.Create()
                .WithIdentity("trigger3", "group1")
                .WithSchedule(CronScheduleBuilder
                    .WeeklyOnDayAndHourAndMinute(DayOfWeek.Wednesday, 10, 42)
                    .InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time")))
                .Build();
            var nextFireTIme = trigger2.GetNextFireTimeUtc();
            for (int i = 0; i < 4; i ++)
            {
                nextFireTIme = trigger2.GetFireTimeAfter(nextFireTIme);
            }
        }
    }
}
