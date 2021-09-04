using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace Lekha.Parser.Tests.Unit
{

    public class ParsetTests
    {
        [Theory]
        //[InlineData("1", false, "-9393,9393,-0.999999999999999999999999999,0.999999999999999999999999999,2021/10/10,This is a string field,19999-12-31T23:59:59")]
        [InlineData("1", true, "Field1,Field2,Field3,Field4,Field5,Field6,Field7\r\n-9393,9393,-0.999999999999999999999999999,0.999999999999999999999999999,2021/10/10,This is a string field,19999-12-31T23:59:59")]
        public async Task ShouldReadRecordStreamWithNoHeaders(string description, bool hasHeader, string lines)
        {
            //
            // Setup
            //
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(lines));
            var sut = new StreamProcessor(new NullLogger<StreamProcessor>());
            var result = await sut.ParseAsync(stream, new FileConfiguration
            {
                RecordConfiguration = new RecordConfiguration
                {
                    Delimiter = StreamProcessor.DefaultDelimiter
                },
                HasHeaderRecord = hasHeader
            });
        }
        public class TestCase
        {
            public decimal TestCaseName { get; set; }
            public string CsvData { get; set; }
            public List<string> TestDataDetails { get; set; }
            public FileConfiguration FileConfiguration { get; set; }
            public TestResult ExpectedResult { get; set; }
        }
        public class TestResult
        {
            [JsonConverter(typeof(ItemListConverter<DictionaryStringObjectJsonConverter, Dictionary<string, object>>))]
            public List<Dictionary<string, object>> RecordsWithDataTypeValues { get; set; }
            public List<Dictionary<string, string>> RecordsWithStringValues { get; set; }
            public string ExceptionType { get; set; }
            public long ProcessedRecordCount { get; set; }
            public long ErrorRecordCount { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        [Theory]
        [InlineData(2.001, "Should parse a single record successfully, detecting comma as delimiter,  with no header and no field configuration specified")]
        [InlineData(2.002, "Should parse a single record, with single field successfully, detecting comma as delimiter, with no header and no field configuration specified")]
        [InlineData(2.003, "Should parse a multiple records, with single field successfully, detecting comma as delimiter,  with no header and no field configuration specified")]
        [InlineData(3, "Should parse a single record successfully with header and field configuration specified")]
        [InlineData(4, "Should parse a single record successfully with no header and field configuration specified")]
        [InlineData(5, "Should parse successfully two records, and detect all fields as 'string' datatype with header specified but no field configuration specified")]
        [InlineData(6, "Should parse a single record successfully and detect all fields as 'string' datatype with header specified but no field configuration specified")]
        [InlineData(7, "Should parse successfully two records, skip additional field and detect all fields as 'string' datatype with header specified but no field configuration specified")]
        [InlineData(8, "Should return and process no records, when data is empty")]
        [InlineData(8.001, "Should parse successfully header specified, but no field configuration is specified, and process empty records, when data is empty")]
        [InlineData(8.002, "Should parse successfully when no header specified, but a field configuration is specified, and process empty records, when blank lines specified")]
        [InlineData(8.003, "Should parse successfully header specified, and field configuration is specified, and process empty records, when data is empty")]
        [InlineData(8.004, "Should parse successfully header specified, and field configuration is specified, and process empty records, when data is blank")]
        [InlineData(9, "Should parse successfully two records, skip additional middle field and detect all fields as 'string' datatype with header specified but no field configuration specified")]
        [InlineData(10, "Should parse and default to a 'string' data type, when no datatype is specified as part of a field configuration")]
        [InlineData(11.001, "Should parse successfully two records, skip additional field, detect and skip first-line comment line, and detect all fields as 'string' datatype with header specified but no field configuration specified")]
        [InlineData(11.002, "Should parse successfully two records, skip additional field, detect and non-first-line skip comment line, and detect all fields as 'string' datatype with header specified but no field configuration specified")]
        [InlineData(12.001, "Should parse successfully two records, detect quoted fields, detect and skip comment line, and detect all fields as 'string' datatype with header specified and field configuration specified")]
        [InlineData(12.002, "Should parse successfully two records, detect quoted fields, detect and skip comment line, detect default separator as 'comma' and detect all fields' datatypes with header specified and field configuration specified")]
        [InlineData(13.001, "Should fail parsing a single record  with header and field configuration specified and a mismatching data type in one of the fields")]
        [InlineData(13.002, "Should fail parsing a single record  with header and field configuration specified and a missing field in the header")]
        [InlineData(13.003, "Should parse successfully a single record  with header and field configuration specified and a missing a 'optional' field in the header")]
        [InlineData(13.004, "Should parse successfully a single record  with header and field configuration and additional field values in the rows with no corresponding header, ignoring additional fields")]
        [InlineData(13.005, "Should parse successfully a multiple records  with header and field configuration and additional field values in the rows with no corresponding header and additional values in records, ignoring additional fields")]
        [InlineData(13.006, "Should parse successfully a multiple records  with no header but field configuration specified, and additional values in records, ignoring additional fields")]
        [InlineData(13.007, "Should parse successfully a multiple records  with header and field configuration and, header having leading and trailing spaces in the name")]
        [InlineData(13.008, "Should parse successfully a multiple records  with header and field configuration and, header and field configuration having leading and trailing spaces in the name")]
        [InlineData(13.009, "Should parse successfully a multiple records  with header and field configuration and, header and field configuration having leading and trailing spaces in the name, and names differ in case sensitivity")]
        [InlineData(13.010, "Should parse successfully a multiple records  with header and field configuration and, header and field configuration having leading and trailing spaces in the name, and names differ in case sensitivity 2")]
        [InlineData(14.000, "Should parse successfully when no header and field configuration specified, and some records have missing values for optional fields")]
        [InlineData(14.001, "Should parse successfully when no header and field configuration specified, and some records have empty values for optional fields")]
        [InlineData(14.002, "Should parse successfully when no field configuration specified, but a header is specified, and some records have empty values for optional fields")]
        [InlineData(14.003, "Should parse successfully when no field configuration specified, but a header is specified, and some records have empty values for optional fields")]
        [InlineData(14.004, "Should parse successfully when no header specified, but a field configuration is specified, and some records have empty values for optional fields")]
        [InlineData(14.005, "Should parse successfully when no header specified, but a field configuration is specified, and some records have empty values for optional fields")]
        [InlineData(14.006, "Should parse successfully when both header and field configuration is specified, and some records have empty values for optional fields")]
        [InlineData(14.007, "Should parse successfully when both header and field configuration is specified, and some records have empty values for optional fields")]
        [InlineData(14.008, "Should fail parsing when no header specified, but a field configuration is specified, and some records have empty values for required fields")]
        [InlineData(14.009, "Should fail parsing when both header and field configuration is specified, and some records have empty values for required fields")]
        [InlineData(15, "Should fail parsing when header but no field configuration is specified, and a header field name is missing")]
        [InlineData(15.1, "Should fail parsing when header but no field configuration is specified, and the same header field is specified more than once")]
        [InlineData(15.2, "Should fail parsing when header but no field configuration is specified, and the same header field is specified more than once, case insensitive")]
        [InlineData(15.3, "Should fail parsing when header but no field configuration is specified, and the same header field is specified more than once")]
        [InlineData(15.4, "Should fail parsing when header but no field configuration is specified, and the same header field is specified more than once, case insensitive")]
        [InlineData(15.5, "Should fail parsing when no header specified, but field configuration is specified, and has a missing field name")]
        [InlineData(15.6, "Should fail parsing when header and field configuration are specified, and missing required fields in the header")]

        public async Task StreamProcessTests(decimal testCaseName, string description)
        {
            //
            // Setup
            //
            var testCaseDataFile = "DataFiles\\TestCase.json";
            var data = File.ReadAllText("DataFiles\\TestCase.json");

            var testCases = JsonSerializer.Deserialize<List<TestCase>>(data);

            var actualResult = new TestResult 
            { 
                RecordsWithDataTypeValues = new List<Dictionary<string, object>>(), 
                RecordsWithStringValues = new List<Dictionary<string, string>>() 
            };

            var testCase = testCases.FirstOrDefault(i => i.TestCaseName == testCaseName);
            if (testCase == null)
            {
                throw new System.Exception($"Error locating test case in test case data file {testCaseDataFile} test.  Make sure there is a test case having {testCaseName} as the TestCaseName");
            }
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testCase.CsvData));
            var sut = new StreamProcessor(new NullLogger<StreamProcessor>());

            //
            // Act
            //
            ParseResult result = null;
            var exception = await Xunit.Record.ExceptionAsync(async () => result = await sut.ParseAsync(stream, testCase.FileConfiguration, null,
                 (long recordIndex, Dictionary<string, object> parsedRecord) =>
                 {
                     if (testCase.ExpectedResult.RecordsWithStringValues.Count > 0)
                     {
                         var convertedDictionary = new Dictionary<string, string>();
                         foreach (var kv in parsedRecord)
                         {
                             convertedDictionary.Add(kv.Key, kv.Value.ToString());
                         }
                         actualResult.RecordsWithStringValues.Add(convertedDictionary);
                     }
                     else
                     {
                         actualResult.RecordsWithDataTypeValues.Add(parsedRecord);
                     }
                     return Task.CompletedTask;
                 },
                 (ParseError error) =>
                 {
                     return Task.FromResult(true);
                 }));
            if (result != null)
            {
                actualResult.ProcessedRecordCount = result.ProcessedRecordCount;
                actualResult.ErrorRecordCount = result.ErrorRecordCount;
                actualResult.Success = result.Success;
                actualResult.Message = result.Message;
            }

            //
            // Verify
            //
            if (string.IsNullOrWhiteSpace(testCase.ExpectedResult.ExceptionType) == false)
            {
                exception.GetType().Name.Should().Be(testCase.ExpectedResult.ExceptionType);
            }
            else
            {
                exception.Should().Be(null);
                actualResult.Should().BeEquivalentTo(testCase.ExpectedResult);
            }
        }

        [Theory]
        [InlineData("1", true, "Field1,Field2,Field3,Field4,Field5,Field6,Field7\r\n-9393,9393,-0.999999999999999999999999999,0.999999999999999999999999999,2021/10/10,This is a string field,9999/12/31 23:59:59")]
        public async Task ShouldReadRecordStreamWithHeaders(string description, bool hasHeader, string lines)
        {
            //
            // Setup
            //
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(lines));
            var sut = new StreamProcessor(new NullLogger<StreamProcessor>());
            var result = await sut.ParseAsync(stream, new FileConfiguration
            {
                RecordConfiguration = new RecordConfiguration
                {
                    Delimiter = StreamProcessor.DefaultDelimiter,
                    Fields = new List<FieldConfiguration>
                    {
                        new FieldConfiguration
                        {
                            Name = "Field1",
                            DataType = FieldType.SignedNumber,
                        },
                        new FieldConfiguration
                        {
                            Name = "Field2",
                            DataType = FieldType.UnsignedNumber,
                        },
                        new FieldConfiguration
                        {
                            Name = "Field3",
                            DataType = FieldType.Decimal,
                        },
                        new FieldConfiguration
                        {
                            Name = "Field4",
                            DataType = FieldType.Decimal,
                        },
                        new FieldConfiguration
                        {
                            Name = "Field5",
                            DataType = FieldType.Date,
                        },
                         new FieldConfiguration
                        {
                            Name = "Field6",
                            DataType = FieldType.String,
                        },
                         new FieldConfiguration
                        {
                            Name = "Field7",
                            DataType = FieldType.Date,
                            DateTimeFormat = "yyyy/MM/dd HH:mm:ss",
                        },
                    }
                },
                HasHeaderRecord = hasHeader
            });
        }

        [Fact]
        public async Task SampleJsonSerializer()
        {
            //
            // Setup
            //
            var data = File.ReadAllText("DataFiles\\TestCase.json");

            var xxx = new FileConfiguration
            {
                HasHeaderRecord = true,
                RecordConfiguration = new RecordConfiguration
                {
                    Delimiter = StreamProcessor.DefaultDelimiter,
                    Fields = new List<FieldConfiguration>
                {
                    new FieldConfiguration
                    {
                        Name = "Field1",
                        DataType = FieldType.SignedNumber,
                    },
                    new FieldConfiguration
                    {
                        Name = "Field2",
                        DataType = FieldType.UnsignedNumber,
                    },
                    new FieldConfiguration
                    {
                        Name = "Field3",
                        DataType = FieldType.Decimal,
                    },
                    new FieldConfiguration
                    {
                        Name = "Field4",
                        DataType = FieldType.Decimal,
                    },
                    new FieldConfiguration
                    {
                        Name = "Field5",
                        DataType = FieldType.Date,
                    },
                    new FieldConfiguration
                    {
                        Name = "Field6",
                        DataType = FieldType.String,
                    },
                    new FieldConfiguration
                    {
                        Name = "Field7",
                        DataType = FieldType.Date,
                        DateTimeFormat = "yyyy/MM/dd HH:mm:ss",
                    },
                }
                },
            };
            var fileConfiguration = JsonSerializer.Serialize(xxx);
            var testCases = JsonSerializer.Deserialize<List<TestCase>>(data);

            var parsedRecords = new TestResult { RecordsWithDataTypeValues = new List<Dictionary<string, object>>() };

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testCases[0].CsvData));
            var sut = new StreamProcessor(new NullLogger<StreamProcessor>());
            var result = await sut.ParseAsync(stream, testCases[0].FileConfiguration, null,
                 (long recordIndex, Dictionary<string, object> parsedRecord) =>
                 {
                     parsedRecords.RecordsWithDataTypeValues.Add(parsedRecord);
                     return Task.CompletedTask;
                 },
                 (ParseError error) =>
                 {
                     return Task.FromResult(true);
                 });
            parsedRecords.RecordsWithDataTypeValues.Should().BeEquivalentTo(testCases[0].ExpectedResult.RecordsWithDataTypeValues);
        }

        public class CompanyOriginal
        {
            public int PrimaryKey { get; set; }
            public int SourceId { get; set; }
            public string SourceName { get; set; }
            public string CompanyName { get; set; }
            public Dictionary<string, string> Extra { get; set; }
        }

        public class CompanyMapOriginal : ClassMap<CompanyOriginal>
        {
            public CompanyMapOriginal(string[] additionalHeaders)
            {
                Map(m => m.PrimaryKey);
                Map(m => m.SourceId);
                Map(m => m.SourceName);
                Map(m => m.CompanyName);

                foreach (var additionalHeader in additionalHeaders)
                {
                    Map(m => m.Extra).Name(additionalHeader);
                }
            }
        }

        [Fact]
        public void CsvDictionaryTestOriginal()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                writer.WriteLine("Extra0,SourceId,PrimaryKey,SourceName,CompanyName,Extra1,Extra2");
                writer.WriteLine("Extra0.1,1,11,Item1,Company1,Extra1.1,Extra2.1");
                writer.WriteLine("Extra0.2,2,22,Item2,Company2,Extra1.2,Extra2.2");
                writer.Flush();
                stream.Position = 0;
                reader.BaseStream.Position = 0;

                csv.Read();
                csv.ReadHeader();
                var headerRow = csv.HeaderRecord;

                csv.Context.RegisterClassMap(new CompanyMapOriginal(headerRow));
                while (csv.Read())
                {
                    var record = csv.GetRecord<CompanyOriginal>();
                    // Do something with the record.
                }
                //var records = csv.GetRecords<Company>().ToList();
            }
        }

        public class Company
        {
            public Dictionary<string, string> Extra { get; set; }
        }

        public class CompanyMap : ClassMap<Company>
        {
            public CompanyMap(string[] additionalHeaders)
            {
                foreach (var additionalHeader in additionalHeaders)
                {
                    Map(m => m.Extra).Name(additionalHeader);
                }
            }
        }

        string PrepareHeaderForMatchx(PrepareHeaderForMatchArgs args)
        {
            return args.Header;
        }
    }
}
