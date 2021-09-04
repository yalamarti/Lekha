using System.Collections.Generic;

namespace Lekha.Parser.Models
{
    public class UploadConfiguration
    {
        public RecordFilter RecordFilter { get; set; }
        public List<UniqueKeyConfiguration> RecordKeys { get; set; }
        public FileConfiguration FileConfiguration { get; set; }
    }

}
