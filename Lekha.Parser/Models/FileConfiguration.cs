namespace Lekha.Parser.Models
{
    /// <summary>
    /// Represents the configuration of a file that is uploaded to the system
    /// </summary>
    public class FileConfiguration
    {
        /// <summary>
        /// Indicates if the file represented by this configuration has a header record.  
        /// Default: false, indicating no header record
        /// </summary>
        public bool? HasHeaderRecord { get; set; }

        /// <summary>
        /// Indicates the comment character to be used.  
        /// Any line that starts with this character is considered a comment line.  
        /// Default: empty, indicting the file represented by this configuration has no comment lines.  
        /// </summary>
        public char? CommentCharacter { get; set; }

        /// <summary>
        /// Configuration of records that are part of the file represented by this configuration
        /// </summary>
        public RecordConfiguration RecordConfiguration { get; set; }
    }

}
