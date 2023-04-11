using Microsoft.Extensions.Logging;
using Polly;

namespace Lekha.Scheduler.Extensions
{
    public static class PollyContextExtensions
    {
        private static readonly string LoggerKey = "LoggerKey";
        public static readonly string DataName = "DataName";

        /// <summary>
        /// Registers a logger with the Polly context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static Context WithLogger(this Context context, ILogger logger)
        {
            context[LoggerKey] = logger;
            return context;
        }

        /// <summary>
        /// Registers data with Polly context.  Typically the name and value used in 
        ///   structured logging during retry attempts
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Context WithData<T>(this Context context, string name, T value)
        {
            context[DataName] = name;
            context[name] = value;
            return context;
        }

        public static ILogger GetLogger(this Context context)
        {
            if (context.TryGetValue(LoggerKey, out object logger))
            {
                return logger as ILogger;
            }
            return null;
        }
    }
}
