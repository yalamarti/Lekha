namespace Lekha.Parser.Models
{
    public static class FieldConfigurationExtensions
    {
        public static string ToMessage(this FieldConfiguration fieldConfiguration, string fieldTextualValue)
        {
            return fieldConfiguration == null ? null : fieldConfiguration.ExposeableToPublic ? $"value:{fieldTextualValue}" : $"value of field:{fieldConfiguration.Name}";
        }
    }

}
