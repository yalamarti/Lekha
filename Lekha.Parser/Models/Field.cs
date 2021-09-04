namespace Lekha.Parser.Models
{
    public class Field
    {
        public string Name { get; set; }
        /// <summary>
        /// 1-based index of the record.
        /// </summary>
        public long? RecordIndex { get; set; }
        /// <summary>
        /// 1-based index of the field
        /// </summary>
        public int? FieldIndex { get; set; }
    }

}
