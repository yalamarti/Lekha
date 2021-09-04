namespace Lekha.Parser.Models
{
    public struct FieldFormats
    {
        public const string DefaultDateFormat = "yyyy/MM/dd";
        public const string DefaultTimeFormat = "hh\\:mm"; // not HH intentionally ... it is hh as the implementaiton uses TimeSpan
        public const string DefaultDateTimeFormat = "yyyy/MM/dd HH:mm";
    }

}
