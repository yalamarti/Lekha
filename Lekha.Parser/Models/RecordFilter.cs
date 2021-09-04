using System.Collections.Generic;

namespace Lekha.Parser.Models
{
    public class RecordFilter
    {
        public string Name { get; set; }
        public List<FilterCondition> Filters { get; set; }
    }

}
