using Lekha.Parser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lekha.Parser
{
    // https://devblogs.microsoft.com/cosmosdb/introducing-bulk-support-in-the-net-sdk/
    public interface IStreamProcessor
    {
        Task<ParseResult> ParseAsync(Stream stream, FileConfiguration fileConfiguration, List<UniqueKeyConfiguration> recordKeys,
            Func<long, Dictionary<string, object>, Task> processedRecordCallback, Func<ParseError, Task<bool>> errorRecordCallback);
    }
}
