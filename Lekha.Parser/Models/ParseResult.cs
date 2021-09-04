using System.Collections.Generic;

namespace Lekha.Parser.Models
{
    public class ParseResult
    {
        public FileConfiguration FileConfiguration { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public long ProcessedRecordCount { get; set; }
        public long ErrorRecordCount { get; set; }
        public long ErrorRecordsIgnored { get; set; }
        public string[] FoundHeaders { get; set; }
        public string[] ExpectedHeaders { get; set; }
        public string[] FieldsWithDuplicateSpecification { get; set; }
        public string[] MissingHeaderFields { get; set; }
        public string[] FieldsWithNoName { get; set; }

        public List<string> Observations { get; set; }

        /// <summary>
        /// Field Configurations that were determined during the parsing of the file
        /// </summary>
        public List<ParseError> Errors { get; set; } = new List<ParseError>();
    }

}
