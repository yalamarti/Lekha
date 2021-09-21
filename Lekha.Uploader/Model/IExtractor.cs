using Lekha.Csv.Converter.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lekha.Uploader.Model
{
    public class FilterCondition
    {
        public string Name { get; set; }
        public string FieldName { get; set; }
        public string Condition { get; set; }
        public string ComparisonValue { get; set; }
    }
    public interface IExtractor
    {
        // Apply filter
        // Remove duplicates
        Task<StringBuilder> Extract(Stream stream);
    }
    public class RecordFilter
    {
        public string Name { get; set; }
        public List<FilterCondition> Filters { get; set; }
    }

    public class UniqueKeyConfiguration
    {
        public string Name { get; set; }
        public List<string> FieldName { get; set; }
    }
    public class UploadConfiguration
    {
        public RecordFilter RecordFilter { get; set; }
        public List<UniqueKeyConfiguration> RecordKeys { get; set; }
        public ConverterConfiguration FileConfiguration { get; set; }
    }
}
