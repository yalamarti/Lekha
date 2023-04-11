using Lekha.Scheduler.BusinessLogic.Models;
using Lekha.Scheduler.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Polly.Retry;
using System;

namespace Lekha.Scheduler.BusinessLogic
{
    public static class Configuration
    {
        private static AsyncRetryPolicy GetRetryPolicy(IConfiguration configuration, int retryCount)
        {
            var defaultBackoff = TimeSpan.FromSeconds(configuration.GetValue<int>(ConfigurationNames.PublishDefaultBackoff, 5));
            var backoffMin = TimeSpan.FromSeconds(configuration.GetValue<int>(ConfigurationNames.PublishBackoffMin, 3));
            var backoffMax = TimeSpan.FromSeconds(configuration.GetValue<int>(ConfigurationNames.PublishBackoffMax, 10));

            return Policy.Handle<Exception>()
                        .WaitAndRetryAsync(retryCount,
                            retryAttempt => GetBackoff(retryAttempt, defaultBackoff, backoffMin, backoffMax),
                            (exception, timeSpan, retryCount, context) =>
                            {
                                var logger = context.GetLogger();
                                object dataToLog = null;
                                string dataName = null;
                                if (context.ContainsKey(PollyContextExtensions.DataName))
                                {
                                    dataName = context[PollyContextExtensions.DataName] as string;
                                    if (string.IsNullOrWhiteSpace(dataName))
                                    {
                                        // null it out if it is just whitespace value - extreme exception case
                                        dataName = null;
                                    }
                                    else
                                    {
                                        dataToLog = context[dataName];
                                    }
                                }
                                const string msg = "Retrying request in {retryTimeSpan} for {operationRetryCount} at {operation}";

                                if (dataName == null)
                                {
                                    logger?.LogWarning(exception, msg + " - no custom data", timeSpan, retryCount, context.OperationKey);
                                }
                                else
                                {
                                    var msg1 = msg + $" with data{{@{dataName}}}";
                                    logger?.LogWarning(exception, msg1, timeSpan, retryCount, context.OperationKey, dataToLog);
                                }
                            }
                        );
        }

        //private static AsyncRetryPolicy GetDataRetrievalRetryPolicy(IConfiguration configuration)
        //{
        //    // Initialize variables with default values  
        //    var defaultBackoff = TimeSpan.FromSeconds(configuration.GetValue<int>(ConfigurationNames.DataRetrievalDefaultBackoff, 5));
        //    var backoffMin = TimeSpan.FromSeconds(configuration.GetValue<int>(ConfigurationNames.DataRetrievalBackoffMin, 3));
        //    var backoffMax = TimeSpan.FromSeconds(configuration.GetValue<int>(ConfigurationNames.DataRetrievalBackoffMax, 10));

        //    return Policy.Handle<Exception>()
        //                .WaitAndRetryAsync(retryCount,
        //                    retryAttempt => GetBackoff(retryAttempt, defaultBackoff, backoffMin, backoffMax),
        //                    (exception, timeSpan, retryCount, context) =>
        //                    {
        //                        var logger = context.GetLogger();
        //                        object dataToLog = null;
        //                        string dataName = null;
        //                        if (context.ContainsKey(PollyContextExtensions.DataName))
        //                        {
        //                            dataName = context[PollyContextExtensions.DataName] as string;
        //                            if (string.IsNullOrWhiteSpace(dataName))
        //                            {
        //                                // null it out if it is just whitespace value - extreme exception case
        //                                dataName = null;
        //                            }
        //                            else
        //                            {
        //                                dataToLog = context[dataName];
        //                            }
        //                        }
        //                        const string msg = "Retrying request for {operationRetryCount} at {operation}, due to: {retryReason} with retryDuration of {retryTimeSpan}";

        //                        if (dataName == null)
        //                        {
        //                            logger?.LogWarning(exception, msg + " - no custom data", retryCount, context.OperationKey, exception, timeSpan);
        //                        }
        //                        else
        //                        {
        //                            var msg1 = msg + $" with data{{@{dataName}}}";
        //                            logger?.LogWarning(exception, msg1, retryCount, context.OperationKey, exception, timeSpan, dataToLog);
        //                        }
        //                    }
        //                );
        //}

        public static void ConfigurePollyPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IReadOnlyPolicyRegistry<string>>(GetPollyPolicyRegistry(configuration));
        }

        public static PolicyRegistry GetPollyPolicyRegistry(IConfiguration configuration)
        {
            PolicyRegistry registry = new PolicyRegistry()
            {
                { PollyPolicies.DataRetrievalRetryPolicy, GetRetryPolicy(configuration, configuration.GetValue<int>(ConfigurationNames.DataRetrievalRetryCount, 5)) },
                { PollyPolicies.PublishMessageRetryPolicy, GetRetryPolicy(configuration, configuration.GetValue<int>(ConfigurationNames.PublishRetryRetryCount, 5)) }
            };
            return registry;
        }

        private static TimeSpan GetBackoff(int retryAttempt)
        {
            //
            // Retry a specified number of times, using a function to 
            //   calculate the duration to wait between retries based on 
            //   the current retry attempt, calling an action on each retry 
            //   with the current exception, duration, retry count, and context 
            //   provided to Execute()
            //     2 ^ 1 = 2 seconds then
            //     2 ^ 2 = 4 seconds then
            //     2 ^ 3 = 8 seconds then
            //     2 ^ 4 = 16 seconds then
            //     2 ^ 5 = 32 seconds
            //
            return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
        }
        /// <summary>
        /// Lifter from https://docs.microsoft.com/en-us/rest/api/storageservices/designing-a-scalable-partitioning-strategy-for-azure-table-storage
        /// </summary>
        /// <param name="retries"></param>
        /// <returns></returns>
        private static TimeSpan GetBackoff(int retryAttempt, TimeSpan defaultBackoff, TimeSpan backoffMin, TimeSpan backoffMax)
        {
            // A recommended retry strategy is one that uses an exponential backoff where each retry attempt
            // is longer than the last attempt.It's similar to the collision avoidance algorithm used in computer
            // networks, such as Ethernet. The exponential backoff uses a random factor to provide an additional
            // variance to the resulting interval. The backoff value is then constrained to minimum and maximum limits.
            // The following formula can be used for calculating the next backoff value using an exponential algorithm:

            //   y = Rand(0.8z, 1.2z)((2 pow x) - 1)
            //   y = Min(zmin + y, zmax)
            //   Where:
            //      z = default backoff in milliseconds
            //      zmin = default minimum backoff in milliseconds
            //      zmax = default maximum backoff in milliseconds
            //      x = the number of retries
            //      y = the backoff value in milliseconds

            //The 0.8 and 1.2 multipliers used in the Rand(random) function produces a random variance of the default backoff within ±20 % of the original value.The ±20 % range is acceptable for most retry strategies, and it prevents further collisions.

            var random = new Random();

            double backoff = random.Next(
                (int)(0.8D * defaultBackoff.TotalMilliseconds),
                (int)(1.2D * defaultBackoff.TotalMilliseconds));
            backoff *= (Math.Pow(2, retryAttempt) - 1);
            backoff = Math.Min(
                backoffMin.TotalMilliseconds + backoff,
                backoffMax.TotalMilliseconds);

            return TimeSpan.FromMilliseconds(backoff);
        }
    }
}
