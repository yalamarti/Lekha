namespace Lekha.Parser.Models
{
    public class FieldConfigurationDto
    {
        public FieldConfiguration Configuration { get; set; }
        public string SanitizedFieldName { get; set; }
        public FieldConfigurationSource FieldConfigSource { get; set; }
    }

}
