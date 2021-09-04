namespace Lekha.Parser.Models
{
    public static class FieldExtensions
    {
        public static string ToSanitizedFieldName(this string fieldName)
        {
            return fieldName.Trim().ToLower();
        }
    }

}
