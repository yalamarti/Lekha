using CsvHelper;
using CsvHelper.Configuration;
using Lekha.Parser.Models;
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
                    logger.LogWarning(ex, "Either no records in the file or there was an error reading the file - while setting up default field configuration");
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
                logger.LogWarning(ex, "Either no records in the file or there was an error reading the file - while validating the header/configuration");
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

            var typeConverters = new Dictionary<FieldConfigurationDto, FieldTypeConverter>();

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
                                Code = ParserErrorName.FieldFormatFailure,
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
                                Code = ParserErrorName.FieldRetrievalFailure,
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
