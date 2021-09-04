using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Lekha.Parser.Tests.Unit
{
    public class JsonConverterTests
    {
        private void RunLogic(Dictionary<string, object> dicationary)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new DictionaryStringObjectJsonConverter() },
                WriteIndented = true
            };
            var text = JsonSerializer.Serialize(dicationary, options);
            var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(text, options);

            deserialized.Should().BeEquivalentTo(dicationary);

            var textFromDeserialized = JsonSerializer.Serialize(deserialized, options);

            textFromDeserialized.Should().Be(text);
        }

        [Fact]
        public void TestJsonConversion1()
        {
            var dicationary = new Dictionary<string, object>
                {
                    {  "1", 9393L },
                    {  "2", new DateTimeOffset(2021, 2, 1, 0, 0, 0, new TimeSpan(0)) },
                    {  "3", 554545.444M },
                    {  "4", 38383838383883.3838M },
                    {  "6", "This is a test value" }
                };
            RunLogic(dicationary);
        }

        [Fact]
        public void TestJsonConversion2()
        {
            var dicationary = new Dictionary<string, object>
                {
                    {  "FieldType.SignedNumber", -9393L },
                    {  "FieldType.UnsignedNumber", 9393L },
                    {  "FieldType.SignedNumberMaxValue", long.MaxValue },
                    {  "FieldType.SignedNumberMinValue", long.MinValue },
                    {  "FieldType.UnsignedNumberMaxValue", ulong.MaxValue },
                    {  "FieldType.UnsignedNumberMinValue", ulong.MinValue },
                    {  "FieldType.DecimalMaxValue", decimal.MaxValue },
                    {  "FieldType.DecimalMinValue", decimal.MinValue },
                    {  "FieldType.DecimalMinPrecision", -0.999999999999999999999999999M },
                    {  "FieldType.DecimalMaxPrecision", 0.999999999999999999999999999M },
                    {  "FieldType.DateTimeMaxValue", new DateTimeOffset(DateTime.MaxValue, new TimeSpan(0)) },
                    {  "FieldType.DateTimeMinValue", new DateTimeOffset(DateTime.MinValue, new TimeSpan(0)) },
                    {  "FieldType.String", "This is a test value" },
                    {  "FieldType.StringResemblingDate", "2021/10/10" },
                    {  "FieldType.StringResemblingDate2", "19999-12-31T23:59:59.9999999+00:00" },
                    {  "FieldType.NullString", null }
                };
            RunLogic(dicationary);
        }

        public class MyInput
        {
            public string Name { get; set; }

            [JsonConverter(typeof(DictionaryStringObjectJsonConverter))]
            public Dictionary<string, object> Data { get; set; }
        }

        [Fact]
        public void TestJsonConversionNestedDictionaryAndList()
        {
            var json = @"{
              ""name"": ""john doe"",
              ""data"": {
                            ""string"": ""string"",
                ""int"": 1,
                ""bool"": true,
                ""date"": ""2020-01-23T00:01:02Z"",
                ""decimal"": 12.345,
                ""null"": null,
                ""array"": [1, 2, 3],
                ""objectArray"": [{
                                ""string"": ""string"",
	                ""int"": 1,
                    ""bool"": true
                  },
                  {
                                ""string"": ""string2"",
                    ""int"": 2,
                    ""bool"": true
                  }
                ],
                ""object"": {
                                ""string"": ""string"",
                  ""int"": 1,
                  ""bool"": true
                }
                        }
                    }
            ";

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var deserialized = JsonSerializer.Deserialize<MyInput>(json, options);

            var textFromDeserialized = JsonSerializer.Serialize(deserialized, options);

            var deserializedAgain = JsonSerializer.Deserialize<MyInput>(json, options);
            var textFromDeserializedAgain = JsonSerializer.Serialize(deserializedAgain, options);

            deserializedAgain.Should().BeEquivalentTo(deserialized);
            textFromDeserializedAgain.Should().Be(textFromDeserialized);
        }

        public class MyInput2
        {
            public string Name { get; set; }

            [JsonConverter(typeof(ItemListConverter<DictionaryStringObjectJsonConverter, Dictionary<string, object>>))]
            public List<Dictionary<string, object>> Data { get; set; }
        }

        [Fact]
        public void TestJsonConversionListOfDictinaryObjects()
        {
            var objectToTest = new MyInput2
            {
                Name = "This is a test object",
                Data = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> {
                        {  "FieldType.SignedNumber", -9393L },
                        {  "FieldType.UnsignedNumber", 9393L },
                        {  "FieldType.SignedNumberMaxValue", long.MaxValue },
                        {  "FieldType.SignedNumberMinValue", long.MinValue },
                        {  "FieldType.UnsignedNumberMaxValue", ulong.MaxValue },
                        {  "FieldType.UnsignedNumberMinValue", ulong.MinValue },
                        {  "FieldType.DecimalMaxValue", decimal.MaxValue },
                        {  "FieldType.DecimalMinValue", decimal.MinValue },
                        {  "FieldType.DecimalMinPrecision", -0.999999999999999999999999999M },
                        {  "FieldType.DecimalMaxPrecision", 0.999999999999999999999999999M },
                        {  "FieldType.DateTimeMaxValue", new DateTimeOffset(DateTime.MaxValue, new TimeSpan(0)) },
                        {  "FieldType.DateTimeMinValue", new DateTimeOffset(DateTime.MinValue, new TimeSpan(0)) },
                        {  "FieldType.String", "This is a test value" },
                        {  "FieldType.StringResemblingDate", "2021/10/10" },
                        {  "FieldType.StringResemblingDate2", "19999-12-31T23:59:59.9999999+00:00" },
                        {  "FieldType.NullString", null }
                    },
                    new Dictionary<string, object> {
                        {  "FieldType.SignedNumber", -9393L },
                        {  "FieldType.UnsignedNumber", 9393L },
                        {  "FieldType.SignedNumberMaxValue", long.MaxValue },
                        {  "FieldType.SignedNumberMinValue", long.MinValue },
                        {  "FieldType.UnsignedNumberMaxValue", ulong.MaxValue },
                        {  "FieldType.UnsignedNumberMinValue", ulong.MinValue },
                        {  "FieldType.DecimalMaxValue", decimal.MaxValue },
                        {  "FieldType.DecimalMinValue", decimal.MinValue },
                        {  "FieldType.DecimalMinPrecision", -0.999999999999999999999999999M },
                        {  "FieldType.DecimalMaxPrecision", 0.999999999999999999999999999M },
                        {  "FieldType.DateTimeMaxValue", new DateTimeOffset(DateTime.MaxValue, new TimeSpan(0)) },
                        {  "FieldType.DateTimeMinValue", new DateTimeOffset(DateTime.MinValue, new TimeSpan(0)) },
                        {  "FieldType.String", "This is a test value" },
                        {  "FieldType.StringResemblingDate", "2021/10/10" },
                        {  "FieldType.StringResemblingDate2", "19999-12-31T23:59:59.9999999+00:00" },
                        {  "FieldType.NullString", null }
                    }
                }

            };
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var deserialized = JsonSerializer.Serialize(objectToTest, options);
            var serialized = JsonSerializer.Deserialize<MyInput2>(deserialized, options);

            serialized.Should().BeEquivalentTo(objectToTest);
        }
    }
}
