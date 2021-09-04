namespace Lekha.Parser.Models
{
    public class ParseError
    {
        public Field Location { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public long? ExpectedFieldCount { get; set; }
        public long? ActualFieldCount { get; set; }
    }

}
