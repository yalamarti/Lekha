using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lekha.Parser
{
    public static class FieldExtensions
    {
        public static string ToSanitizedFieldName(this string fieldName)
        {
            return fieldName.Trim().ToLower();
        }
    }
    /// <summary>
    /// Represents the configuration of a field within record of a file that is uploaded to the system
    /// </summary>
    public class FieldConfiguration
    {
        /// <summary>
        /// 1- based index of the field in the record.  Optional. Default: 0 - indicates index not applicable.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Name of the field.  Optional. When not specified, implies name from HeaderRecords or a system generated name.
        /// Case insensitive: (meaning case of the letters in the name doesn't matter when comparing with, 
        /// say, comparing the field name to a corresponding field header in a data file)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A displayable title for the field.  Optional. Default: Same as Name value.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Data type of the field - valid values: number, unsigned-number, decimal, date, datetime, time, string.  Default: string
        ///   https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types: 
        ///      Number:   long      System.Int64    Size: Signed 64-bit integer
        ///         Range: -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807
        ///         
        ///      Unsigned-Number:   ulong      System.UInt64    Size: Unsigned Signed 64-bit integer
        ///         Range: 0 to 18,446,744,073,709,551,615
        ///         
        ///      Decimal : decimal 	System.Decimal 	Size: 16 bytes 	
        ///         Precision: - 28-29 decimal places 	(28-29: includes significant digits and decimal places)
        ///         Range:     +-1.0 x 10 power 28 to +-7.9 x 10 power 28
        ///         
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Format of the date, datetime or time field value.  Optional.  Default: For date - 'yyyy/MM/dd'.  For time: 'hh\:mm'.  For datetime: 'yyyy/MM/dd HH:mm' 
        /// </summary>
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Indicates if the field value can be empty or blank/spaces
        /// </summary>
        public bool AllowEmptyField { get; set; }

        /// <summary>
        /// Indicates if the field value has to be specified as part of the record.
        /// Applies when a header record is specified.
        /// When value is 'true', in case the field value is missing in a record, the record will be marked as 'in error'
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Maximum allowed length of the field
        /// </summary>
        public int AllowedMaximumLength { get; set; } = FieldLimits.MaximumLength;

        /// <summary>
        /// Number of decima places for a field representing a decimal value
        /// </summary>
        public int DecimalPlaces { get; set; }

        public bool ExposeableToPublic { get; set; }
    }

    public class FieldConfigurationDto
    {
        public FieldConfiguration Configuration { get; set; }
        public string SanitizedFieldName { get; set; }
        public FieldConfigurationSource FieldConfigSource { get; set; }
    }
    public static class FieldConfigurationExtensions
    {
        public static string ToMessage(this FieldConfiguration fieldConfiguration, string fieldTextualValue)
        {
            return fieldConfiguration == null ? null : fieldConfiguration.ExposeableToPublic ? $"value:{fieldTextualValue}" : $"value of field:{fieldConfiguration.Name}";
        }
    }

    /// <summary>
    /// Represents the configuration of a record within a file that is uploaded to the system
    /// </summary>
    public class RecordConfiguration
    {
        /// <summary>
        /// Field delimiter within a record.
        /// Default: comma ","
        /// </summary>
        public string Delimiter { get; set; }

        /// <summary>
        /// Field configuration specification for the fields in a record
        /// </summary>
        public List<FieldConfiguration> Fields { get; set; }
    }

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
    public struct FilterConditionalOperation
    {
        public const string EqualTo = "=";
        public const string LessThan = "<";
        public const string GreaterThan = ">";
        public const string LessThanOrEqualTo = "<=";
        public const string GreaterThanOrEqualTo = ">=";
        public const string Matches = "matches";
        public const string In = "In";
    }
    public struct FieldType
    {
        public const string String = "string";
        public const string SignedNumber = "number";
        public const string UnsignedNumber = "unsigned-number";
        public const string Decimal = "decimal";
        public const string Date = "date";
        public const string Time = "time";
        public const string DateTime = "datetime";
    }
    public struct FieldLimits
    {
        public const int MaximumLength = 2048;
    }
    public struct FieldDelimiter
    {
        public const char Comma = ',';
    }
    public struct FieldFormats
    {
        public const string DefaultDateFormat = "yyyy/MM/dd";
        public const string DefaultTimeFormat = "hh\\:mm"; // not HH intentionally ... it is hh as the implementaiton uses TimeSpan
        public const string DefaultDateTimeFormat = "yyyy/MM/dd HH:mm";
    }
    public class FilterCondition
    {
        public string Name { get; set; }
        public string FieldName { get; set; }
        public string Condition { get; set; }
        public string ComparisonValue { get; set; }
    }


    public class UniqueKeyConfiguration
    {
        public string Name { get; set; }
        public List<string> FieldName { get; set; }
    }

    public class RecordFilter
    {
        public string Name { get; set; }
        public List<FilterCondition> Filters { get; set; }
    }

    public class UploadConfiguration
    {
        public RecordFilter RecordFilter { get; set; }
        public List<UniqueKeyConfiguration> RecordKeys { get; set; }
        public FileConfiguration FileConfiguration { get; set; }
    }

    public class ParseError
    {
        public Field Location { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public long? ExpectedFieldCount { get; set; }
        public long? ActualFieldCount { get; set; }
    }

    public class ParseResult
    {
        public FileConfiguration FileConfiguration { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public long ProcessedRecordCount { get; set; }
        public long ErrorRecordCount { get; set; }
        public long ErrorRecordsIgnored { get; set; }
        public string[] FoundHeaders { get; set; }
        public string[] ExpectedHeaders { get; set; }
        public string[] FieldsWithDuplicateSpecification { get; set; }
        public string[] MissingHeaderFields { get; set; }
        public string[] FieldsWithNoName { get; set; }

        public List<string> Observations { get; set; }

        /// <summary>
        /// Field Configurations that were determined during the parsing of the file
        /// </summary>
        public List<ParseError> Errors { get; set; } = new List<ParseError>();
    }

    public struct ParserError
    {
        public const string FieldCountMismatch = "FieldCountMismatch";
        public const string FieldRetrievalFailure = "FieldRetrievalFailure";
        public const string FieldFormatFailure = "FieldFormatFailure";
        public const string FieldRequiredUnxpectedError = "FieldRequiredUnxpectedError";
    }
    public interface IStreamProcessor
    {
        Task<ParseResult> ParseAsync(Stream stream, FileConfiguration fileConfiguration, List<UniqueKeyConfiguration> recordKeys,
            Func<long, Dictionary<string, object>, Task> processedRecordCallback, Func<ParseError, Task<bool>> errorRecordCallback);
    }

    public interface IExtractor
    {
        // Apply filter
        // Remove duplicates
        Task<StringBuilder> Extract(Stream stream);
    }

    public interface ITransformer
    {
        // Tranform to format usable by Action Executor
        Task<StringBuilder> Transform(Stream stream);
    }

    public interface ILoader
    {
        // Load transformed data to the target data store for later retrieval
        Task<StringBuilder> Load(Stream stream);
    }

    public class FieldTypeConverterException : Exception
    {
        public FieldTypeConverterException(string message) : base(message)
        {
        }
    }

    public class ParserException : Exception
    {
        public ParserException(string message) : base(message)
        {
        }
    }
    public class FieldTypeConverter : ITypeConverter
    {
        private readonly FieldConfiguration fieldConfiguration;

        public FieldTypeConverter(FieldConfiguration fieldConfiguration)
        {
            this.fieldConfiguration = fieldConfiguration;
        }


        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            switch (fieldConfiguration.DataType)
            {
                case FieldType.String:
                    if (string.IsNullOrWhiteSpace(text) == false && text.Length > fieldConfiguration.AllowedMaximumLength)
                    {
                        throw new FieldTypeConverterException($"Failed to convert '{fieldConfiguration.ToMessage(text)}' to {FieldType.SignedNumber}.  Exceeds maximum allowed length of {fieldConfiguration.AllowedMaximumLength}");
                    }
                    return text;
                case FieldType.SignedNumber:
                    {
                        if (string.IsNullOrWhiteSpace(text) && fieldConfiguration.AllowEmptyField)
                        {
                            return null;
                        }
                        if (long.TryParse(text, out long value) == false)
                        {
                            throw new FieldTypeConverterException($"Failed to convert '{fieldConfiguration.ToMessage(text)}' to {FieldType.SignedNumber}");
                        }
                        return value;
                    }
                case FieldType.UnsignedNumber:
                    {
                        if (string.IsNullOrWhiteSpace(text) && fieldConfiguration.AllowEmptyField)
                        {
                            return null;
                        }
                        if (ulong.TryParse(text, out ulong value) == false)
                        {
                            throw new FieldTypeConverterException($"Failed to convert '{fieldConfiguration.ToMessage(text)}' to {FieldType.UnsignedNumber}");
                        }
                        return value;
                    }
                case FieldType.Decimal:
                    {
                        if (string.IsNullOrWhiteSpace(text) && fieldConfiguration.AllowEmptyField)
                        {
                            return null;
                        }
                        if (decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal value) == false)
                        {
                            throw new FieldTypeConverterException($"Failed to convert '{fieldConfiguration.ToMessage(text)}' to {FieldType.Decimal}");
                        }
                        return value;
                    }
                case FieldType.Date:
                    {
                        if (string.IsNullOrWhiteSpace(text) && fieldConfiguration.AllowEmptyField)
                        {
                            return null;
                        }
                        if (DateTimeOffset.TryParseExact(text,
                            string.IsNullOrWhiteSpace(fieldConfiguration.DateTimeFormat) ? FieldFormats.DefaultDateFormat : fieldConfiguration.DateTimeFormat,
                            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset value) == false)
                        {
                            if (string.IsNullOrWhiteSpace(fieldConfiguration.DateTimeFormat))
                                throw new FieldTypeConverterException($"Failed to convert '{fieldConfiguration.ToMessage(text)}' to {FieldType.Date} using '{FieldFormats.DefaultDateFormat}' format");
                            else
                                throw new FieldTypeConverterException($"Failed to convert '{fieldConfiguration.ToMessage(text)}' to {FieldType.Date} using '{fieldConfiguration.DateTimeFormat}' format");
                        }
                        return value;
                    }
                case FieldType.Time:
                    {
                        if (string.IsNullOrWhiteSpace(text) && fieldConfiguration.AllowEmptyField)
                        {
                            return null;
                        }

                        if (TimeSpan.TryParseExact(text,
                            string.IsNullOrWhiteSpace(fieldConfiguration.DateTimeFormat) ? FieldFormats.DefaultTimeFormat : fieldConfiguration.DateTimeFormat,
                            CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan value) == false)
                        {
                            if (string.IsNullOrWhiteSpace(fieldConfiguration.DateTimeFormat))
                                throw new FieldTypeConverterException($"Failed to convert '{fieldConfiguration.ToMessage(text)}' to {FieldType.Time} using '{FieldFormats.DefaultTimeFormat}' format");
                            else
                                throw new FieldTypeConverterException($"Failed to convert '{fieldConfiguration.ToMessage(text)}' to {FieldType.Date} using '{fieldConfiguration.DateTimeFormat}' format");
                        }
                        return value;
                    }
                case FieldType.DateTime:
                    {
                        if (string.IsNullOrWhiteSpace(text) && fieldConfiguration.AllowEmptyField)
                        {
                            return null;
                        }
                        if (DateTimeOffset.TryParseExact(text,
                            string.IsNullOrWhiteSpace(fieldConfiguration.DateTimeFormat) ? FieldFormats.DefaultDateTimeFormat : fieldConfiguration.DateTimeFormat,
                            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset value) == false)
                        {
                            if (string.IsNullOrWhiteSpace(fieldConfiguration.DateTimeFormat))
                                throw new FieldTypeConverterException($"Failed to convert '{fieldConfiguration.ToMessage(text)}' to {FieldType.Date} using '{FieldFormats.DefaultDateFormat}' format");
                            else
                                throw new FieldTypeConverterException($"Failed to convert '{fieldConfiguration.ToMessage(text)}' to {FieldType.Date} using '{fieldConfiguration.DateTimeFormat}' format");
                        }
                        return value;
                    }
                default:
                    {
                        throw new FieldTypeConverterException($"ConvertFromString: Invalid Field Type {fieldConfiguration.DataType} specified for '{fieldConfiguration.ToMessage(text)}'! Valid field types are : " +
                            $"{FieldType.String},{FieldType.SignedNumber},{FieldType.UnsignedNumber},{FieldType.Decimal},{FieldType.Date},{FieldType.Time},{FieldType.DateTime}");
                    }
            }
        }

        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            switch (fieldConfiguration.DataType)
            {
                case FieldType.String:
                    if (value?.ToString()?.Length > fieldConfiguration.AllowedMaximumLength)
                    {
                        throw new FieldTypeConverterException($"Failed to convert '{value}' to a {fieldConfiguration.DataType}.  Exceeds maximum allowed length of {fieldConfiguration.AllowedMaximumLength}");
                    }
                    return value?.ToString();
                case FieldType.SignedNumber:
                case FieldType.UnsignedNumber:
                    if (value == null && fieldConfiguration.AllowEmptyField == false)
                    {
                        throw new FieldTypeConverterException($"Failed to convert '{value}' to a {fieldConfiguration.DataType}.  Value is empty");
                    }
                    return value?.ToString();
                case FieldType.Decimal:
                    if (value == null && fieldConfiguration.AllowEmptyField == false)
                    {
                        throw new FieldTypeConverterException($"Failed to convert '{value}' to a {fieldConfiguration.DataType}.  Value is empty");
                    }
                    return value == null ? null : ((decimal)(value)).ToString("G", CultureInfo.InvariantCulture);
                case FieldType.Date:
                case FieldType.DateTime:
                    if (value == null && fieldConfiguration.AllowEmptyField == false)
                    {
                        throw new FieldTypeConverterException($"Failed to convert '{value}' to a {fieldConfiguration.DataType}.  Value is empty");
                    }
                    return value == null ? null : ((DateTimeOffset)(value)).ToString(fieldConfiguration.DateTimeFormat);
                case FieldType.Time:
                    if (value == null && fieldConfiguration.AllowEmptyField == false)
                    {
                        throw new FieldTypeConverterException($"Failed to convert '{value}' to a {fieldConfiguration.DataType}.  Value is empty");
                    }
                    return value == null ? null : ((TimeSpan)(value)).ToString(fieldConfiguration.DateTimeFormat);
                default:
                    {
                        throw new Exception($"ConvertToString: Invalid Field Type {fieldConfiguration.DataType} specified for value '{value}'! Valid field types are : " +
                            $"{FieldType.String},{FieldType.SignedNumber},{FieldType.UnsignedNumber},{FieldType.Decimal},{FieldType.Date},{FieldType.Time},{FieldType.DateTime}");
                    }
            }
        }
    }
    public class RecordMap : ClassMap<Record>
    {
        public RecordMap(RecordConfiguration recordConfiguration)
        {
            int index = 0;
            foreach (var field in recordConfiguration.Fields)
            {
                Map(m => m.Extra)
                    .Name(field.Name)
                    .TypeConverter(new FieldTypeConverter(field));
            }
        }
    }
    public class Record
    {
        public Dictionary<string, object> Extra { get; set; }
    }

    public enum FieldConfigurationSource
    {
        FromHeader,
        FromFieldConfiguration,
        AutoGenerated
    }
    // https://devblogs.microsoft.com/cosmosdb/introducing-bulk-support-in-the-net-sdk/
    public class StreamProcessor : IStreamProcessor
    {
        public const string NoRecordsProcessed = "No records processed";
        public const string DefaultDelimiter = ",";
        const int MaximumFieldCountAllowed = 20000;
        private readonly ILogger<StreamProcessor> logger;

        public StreamProcessor(ILogger<StreamProcessor> logger)
        {
            this.logger = logger;
        }

        private FieldConfiguration NewFieldConfiguration(int index)
        {
            var name = $"Field{index}";
            return NewFieldConfiguration(name, index);
        }
        private FieldConfiguration NewFieldConfiguration(string name, int index)
        {
            return new FieldConfiguration
            {
                Name = name,
                DataType = FieldType.String,
                AllowedMaximumLength = FieldLimits.MaximumLength,
                AllowEmptyField = true,
                Required = false,
                Title = name,
                Index = index
            };
        }
        private (ParseResult ParseResult, List<FieldConfiguration> FieldConfigurations) SetupDefaultFieldConfigurations(Stream stream, bool hasHeaderRecord)
        {
            using var reader = new StreamReader(stream, Encoding.Default, false, 1024, true);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

            var retVal = new List<FieldConfiguration>();

            csvReader.Read();
            if (hasHeaderRecord)
            {
                var index = 1;
                try
                {
                    csvReader.ReadHeader();
                }
                catch (ReaderException ex)
                {
                    return (new ParseResult
                    {
                        Success = true,
                        Message = NoRecordsProcessed,
                        Observations = new List<string> { "Either no records in the file or there was an error reading the file." }
                    }, retVal);
                }
                foreach (var header in csvReader.HeaderRecord)
                {
                    var fieldConfiguration = NewFieldConfiguration(header, index);
                    retVal.Add(fieldConfiguration);
                    index++;
                    logger.LogInformation("Header field {FieldName} at index {FieldIndex} with default properties", fieldConfiguration.Name, fieldConfiguration.Index);
                }
            }
            else
            {
                for (int index = 0; index <= MaximumFieldCountAllowed; index++)
                {
                    var value = csvReader.TryGetField(index, out string fieldValue);
                    if (value)
                    {
                        var fieldConfiguration = NewFieldConfiguration(index + 1);
                        retVal.Add(fieldConfiguration);
                        logger.LogInformation("Header field {FieldName} at index {FieldIndex} with default properties", fieldConfiguration.Name, index);
                    }
                    else
                    {
                        logger.LogInformation("Determined header as having {FieldCount} from the first record in the stream", index);
                        break;
                    }
                }

            }
            return (null, retVal);
        }

        private ParseResult ValidateHeaderAndFieldConfiguration(Stream stream, List<FieldConfigurationDto> sanitizedFieldConfigurations,
            bool hasHeaderRecord, bool userSpecifiedFieldConfigurations)
        {
            var requiredFieldCount = sanitizedFieldConfigurations
                .Where(i => i.Configuration.Required == true)
                .Count();
            if (userSpecifiedFieldConfigurations && hasHeaderRecord == false && requiredFieldCount > 0)
            {
                var requiredFieldNames = sanitizedFieldConfigurations
                    .Where(i => i.Configuration.Required == true)
                    .Select(i => i.Configuration.Name);
                var fieldNames = string.Join(",", requiredFieldNames.ToArray());
                return new ParseResult
                {
                    Message = $"Field(s) '{fieldNames}' are configured as 'Required'.  'Required' fields are supported only when the '{nameof(FileConfiguration.HasHeaderRecord)}' is set to true and the data file has a header record."
                };
            }

            using var reader = new StreamReader(stream, Encoding.Default, false, 1024, true);
            using var csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { IgnoreBlankLines = true });

            var retVal = new List<FieldConfiguration>();
            string[] headerRowFromStream = null;

            try
            {
                csvReader.Read();
                csvReader.ReadHeader();
            }
            catch (ReaderException ex)
            {
                return new ParseResult
                {
                    Success = true,
                    Message = NoRecordsProcessed,
                    Observations = new List<string> { "Either no records in the file or there was an error reading the file." }
                };
            }

            headerRowFromStream = csvReader.HeaderRecord;
            if (headerRowFromStream == null || headerRowFromStream.Length == 0)
            {
                // No records in the file
                // Nothing to process
                return new ParseResult
                {
                    Success = true,
                    Message = NoRecordsProcessed
                };
            }

            var fieldConfigurationSource = sanitizedFieldConfigurations.FirstOrDefault()?.FieldConfigSource;
            // Validate if Field info specified in file matches with actual Header fields
            var fieldsWithNoName = sanitizedFieldConfigurations
                .Where(i => string.IsNullOrWhiteSpace(i.SanitizedFieldName));
            if (fieldsWithNoName.Count() > 0)
            {
                return new ParseResult
                {
                    Message = 
                        fieldConfigurationSource == FieldConfigurationSource.FromFieldConfiguration ?
                            "File Record Configuration - field with no name specified found!  When one or more fields are specified, Field Name is required for every field!"
                            : "Header - field with no name specified found!  When one or more fields are specified, Field Name is required for every field!"
                };
            }

            // Validate if Field info specified in file matches with actual Header fields
            var duplicateFields = sanitizedFieldConfigurations
                .GroupBy(s => s.SanitizedFieldName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
            if (duplicateFields.Count() > 0)
            {
                string names = string.Join(",", duplicateFields);
                return new ParseResult
                {
                    Message =
                        fieldConfigurationSource == FieldConfigurationSource.FromFieldConfiguration 
                        ? $"File Record Configuration - field with same name(s) '{names}' appears more than once"
                        : $"Header - field with same name(s) '{names}' appears more than once",
                    FieldsWithDuplicateSpecification = duplicateFields.ToArray()
                };
            }

            //var requiredFields = sanitizedFieldConfigurations
            //   .Where(i => i.Configuration.Required == true)
            //   .Select(i => i.SanitizedFieldName);
            //if (headerRowFromStream == null || requiredFields.Count() > headerRowFromStream.Length)
            //{
            //    return new ParseResult
            //    {
            //        Message = "Header expectation - header field count mismatch with field specification in record configuration",
            //        FoundHeaders = headerRowFromStream,
            //        ExpectedHeaders = sanitizedFieldConfigurations.Select(i => i.Configuration.Name).ToArray(),
            //    };
            //}

            //
            // Expected header record specified by the user
            //

            var missingHeaderFieldInStream = new List<string>();
            var sanitizedFieldsRequired = sanitizedFieldConfigurations
                .Where(i => i.Configuration.Required == true);
            foreach (var sanitizedFieldRequired in sanitizedFieldsRequired)
            {
                if (headerRowFromStream.FirstOrDefault(i => i.ToSanitizedFieldName() == sanitizedFieldRequired.SanitizedFieldName) == null)
                {
                    missingHeaderFieldInStream.Add(sanitizedFieldRequired.Configuration.Name);
                }
            }
            if (missingHeaderFieldInStream.Count > 0)
            {
                var fieldNames = string.Join(",", missingHeaderFieldInStream.ToArray());

                return new ParseResult
                {
                    Message = $"Field(s) '{fieldNames}' specified in Record Configuration but are missing in the header data",
                    FoundHeaders = headerRowFromStream,
                    ExpectedHeaders = sanitizedFieldsRequired.Select(i => i.Configuration.Name).ToArray(),
                    MissingHeaderFields = missingHeaderFieldInStream.ToArray()
                };
            }

            return null;
        }

        public Task<ParseResult> ParseAsync(Stream stream)
        {
            // Setup default file configuration
            var fileConfiguration = new FileConfiguration
            {
                RecordConfiguration = new RecordConfiguration
                {
                    Delimiter = DefaultDelimiter,
                    Fields = new List<FieldConfiguration>(),
                }
            };

            return ParseAsync(stream, fileConfiguration);
        }

        public Task<ParseResult> ParseAsync(Stream stream, FileConfiguration fileConfiguration)
        {
            if (fileConfiguration.RecordConfiguration == null)
            {
                fileConfiguration.RecordConfiguration = new RecordConfiguration
                {
                    Delimiter = DefaultDelimiter,
                    Fields = new List<FieldConfiguration>(),
                };
            }
            return ParseAsync(stream, fileConfiguration, null, null, null);
        }

        private List<FieldConfigurationDto> SanitizedFieldInfo(List<FieldConfiguration> fields, FieldConfigurationSource fieldConfigurationSource)
        {
            var retVal = new List<FieldConfigurationDto>();
            foreach (var field in fields)
            {
                retVal.Add(new FieldConfigurationDto
                {
                    Configuration = field,
                    SanitizedFieldName = field.Name.ToSanitizedFieldName(),
                    FieldConfigSource = fieldConfigurationSource
                });
            }
            return retVal;
        }

        private void SetupObservations(FileConfiguration fileConfiguration, ParseResult parseResult)
        {
            if (fileConfiguration.CommentCharacter == null)
            {
                parseResult.Observations.Add("No comment character specified.  Assuming no commented lines.");
            }
            else
            {
                parseResult.Observations.Add($"Comment character specified.  Lines starting with '{fileConfiguration.CommentCharacter.Value}' character will be ignored.");
            }
            if (string.IsNullOrWhiteSpace(fileConfiguration.RecordConfiguration?.Delimiter))
            {
                parseResult.Observations.Add("No field delimiter specified.  Delimiter will be auto-detected.");
            }
        }

        private CsvConfiguration SetupCsvReaderConfiguration(FileConfiguration fileConfiguration)
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = (fileConfiguration.HasHeaderRecord == true)
            };
            if (fileConfiguration.CommentCharacter != null)
            {
                csvConfiguration.Comment = fileConfiguration.CommentCharacter.Value;
                csvConfiguration.AllowComments = true;
            }

            if (string.IsNullOrWhiteSpace(fileConfiguration.RecordConfiguration?.Delimiter))
            {
                csvConfiguration.DetectDelimiter = true;
            }

            csvConfiguration.PrepareHeaderForMatch += new PrepareHeaderForMatch((PrepareHeaderForMatchArgs args) =>
            {
                return args.Header?.ToSanitizedFieldName();
            });

            return csvConfiguration;
        }

        public async Task<ParseResult> ParseAsync(Stream stream, FileConfiguration fileConfiguration, List<UniqueKeyConfiguration> recordKeyConfigurations,
            Func<long, Dictionary<string, object>, Task> processedRecordCallback, Func<ParseError, Task<bool>> errorCallback)
        {
            var result = new ParseResult
            {
                Observations = new List<string>()
            };

            #region Setup and validate input
            if (fileConfiguration == null)
            {
                fileConfiguration = new FileConfiguration
                {
                };
                // No header
                result.Observations.Add("No header specified.  Defaulting to 'No Header'.");
            }
            result.FileConfiguration = fileConfiguration;

            if (fileConfiguration.RecordConfiguration == null)
            {
                fileConfiguration.RecordConfiguration = new RecordConfiguration
                {
                };
                result.Observations.Add($"No Field specification found.  Will default all fields to be of {FieldType.String} data type.");
            }

            var userSpecifiedFieldConfigurations = fileConfiguration.RecordConfiguration?.Fields?.Count > 0;

            bool hasHeaderRecord = (fileConfiguration.HasHeaderRecord == true);

            FieldConfigurationSource fieldInfoSource = FieldConfigurationSource.FromFieldConfiguration;

            if (userSpecifiedFieldConfigurations == false)
            {
                if (hasHeaderRecord)
                {
                    fieldInfoSource = FieldConfigurationSource.FromHeader;
                    result.Observations.Add("No fields specified as part of FileConfiguration.  Fields wiill be auto-detected.");
                }
                else
                {
                    fieldInfoSource = FieldConfigurationSource.AutoGenerated;
                    result.Observations.Add($"No header in file or fields specified as part of FileConfiguration.  Header and fields will be auto-detected.  All fields will be of {FieldType.String} type.");
                }
                (ParseResult ParseResult, List<FieldConfiguration> FieldConfigurations) configResult = SetupDefaultFieldConfigurations(stream, hasHeaderRecord);
                if (configResult.ParseResult != null)
                {
                    return configResult.ParseResult;
                }
                fileConfiguration.RecordConfiguration.Fields = configResult.FieldConfigurations;
                stream.Position = 0;
            }
            List<FieldConfigurationDto> sanitizedFieldConfigurations = SanitizedFieldInfo(fileConfiguration.RecordConfiguration.Fields, fieldInfoSource);

            CsvConfiguration csvConfiguration = SetupCsvReaderConfiguration(fileConfiguration);
            var parseResult = ValidateHeaderAndFieldConfiguration(stream, sanitizedFieldConfigurations, hasHeaderRecord, userSpecifiedFieldConfigurations);
            if (parseResult != null)
            {
                return parseResult;
            }
            stream.Position = 0;

            //
            // Set defaults for missing attributes
            //

            foreach (var sanitizedField in sanitizedFieldConfigurations)
            {
                if (string.IsNullOrWhiteSpace(sanitizedField.Configuration.DataType))
                {
                    sanitizedField.Configuration.DataType = FieldType.String;
                }
            }

            //
            // Setup CSV Reader configuration
            //
            SetupObservations(fileConfiguration, result);

            Dictionary<FieldConfigurationDto, FieldTypeConverter> typeConverters = new Dictionary<FieldConfigurationDto, FieldTypeConverter>();

            foreach (var field in sanitizedFieldConfigurations)
            {
                typeConverters[field] = new FieldTypeConverter(field.Configuration);
            }

            #endregion Setup and validate input

            #region Parse

            using var reader = new StreamReader(stream);
            using var csvReader = new CsvReader(reader, csvConfiguration);

            ParseError error = null;
            var fieldIndex = 0;
            bool parseFieldToValue = false;

            if (hasHeaderRecord)
            {
                csvReader.Read();
                csvReader.ReadHeader();
            }

            // 
            // Note.  Tried the CsvReader's RecordMap way of mapping the record to a Dictionary<string, object>
            //   but ran into issues.  Could not figure out how to map to individual elements ... all the
            //   examples referred to class/property-name based mapping, not dictionary-key mapping
            //   So, parsing individual field 'by hand' here.
            //

            Dictionary<string, object> parsedRecord = null;

            while (csvReader.Read())
            {
                error = null;
                fieldIndex = 0;
                parsedRecord = new Dictionary<string, object>();

                #region Parse individual record
                error = null;

                foreach (var sanitizedFieldDto in sanitizedFieldConfigurations)
                {
                    object objectValue = null;
                    parseFieldToValue = false;

                    try
                    {
                        if (hasHeaderRecord)
                        {
                            objectValue = csvReader.GetField<object>(sanitizedFieldDto.SanitizedFieldName, typeConverters[sanitizedFieldDto]);
                        }
                        else
                        {
                            // User specified configurations have datatype specified.  try getting the field as 'object'
                            objectValue = csvReader.GetField<object>(fieldIndex, typeConverters[sanitizedFieldDto]);
                        }
                        parseFieldToValue = true;
                    }
                    catch (FieldTypeConverterException ex)
                    {
                        var fieldIndexToReport = csvReader.CurrentIndex + 1;
                        bool emptyValuedOptionalField = false;
                        if (sanitizedFieldDto.Configuration.Required == false)
                        {
                            var stringValue = csvReader.GetField<string>(fieldIndex);
                            emptyValuedOptionalField = string.IsNullOrWhiteSpace(stringValue);
                        }
                        if (emptyValuedOptionalField)
                        {
                            result.Observations.Add($"Found an empty optional field at index {fieldIndexToReport} with name {sanitizedFieldDto.Configuration.Name} on record {csvReader.Context.Parser.Row}");
                        }
                        else
                        {
                            error = new ParseError
                            {
                                Location = new Field
                                {
                                    Name = sanitizedFieldDto.SanitizedFieldName,
                                    RecordIndex = csvReader.Context.Parser.Row,
                                    FieldIndex = csvReader.CurrentIndex + 1
                                },
                                Code = ParserError.FieldFormatFailure,
                                Message = ex.Message
                            };
                            logger.LogError(ex, "Error parsing field {FieldError}", error);
                        }
                    }
                    catch (CsvHelper.MissingFieldException ex)
                    {
                        var fieldIndexToReport = csvReader.CurrentIndex + 1;
                        if (sanitizedFieldDto.Configuration.Required)
                        {
                            error = new ParseError
                            {
                                Location = new Field
                                {
                                    Name = sanitizedFieldDto.Configuration.Name,
                                    RecordIndex = csvReader.Context.Parser.Row,
                                    FieldIndex = fieldIndexToReport
                                },
                                Code = ParserError.FieldRetrievalFailure,
                                Message = "Missing required field"
                            };
                            logger.LogError(ex, "Error parsing field {FieldError}", error);
                        }
                        else
                        {
                            result.Observations.Add($"Missing optional field at index {fieldIndexToReport} with name {sanitizedFieldDto.Configuration.Name} on record {csvReader.Context.Parser.Row}");
                        }
                    }

                    if (parseFieldToValue)
                    {
                        parsedRecord[sanitizedFieldDto.Configuration.Name] = objectValue;
                    }
                    else if (error != null)
                    {
                        result.Errors.Add(error);
                        result.ErrorRecordCount++;
                        break;
                    }

                    fieldIndex++;
                }

                result.ProcessedRecordCount++;

                #endregion Parse individual record

                #region Act on parsed record

                if (error == null)
                {
                    // Do something with the record.
                    if (processedRecordCallback != null)
                    {
                        await processedRecordCallback(result.ProcessedRecordCount, parsedRecord);
                    }
                }
                else
                {
                    var continueParsing = await errorCallback(error);
                    if (continueParsing == false)
                    {
                        break;
                    }
                }

                #endregion Act on parsed record
            }

            #endregion Parse

            result.Success = result.Errors.Count == 0;
            result.Message = string.IsNullOrWhiteSpace(result.Message) ? (result.ProcessedRecordCount == 0 ? NoRecordsProcessed : null) : result.Message; return result;

            // Task-In-Motion
            //    TIMO framework
            // Task Delivery Framework
            //    T-Del Framework

            // TODO : with speciical character in header field
            //  -with special characters in records

            // TODO: All other exception - CSVReaderException and other CSVReader exceptions
        }
    }
}
