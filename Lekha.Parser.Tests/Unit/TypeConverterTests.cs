using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Lekha.Parser.Tests.Unit
{

    public class TypeConverterTests
    {
        [Theory]
        [InlineData("1", "aaa", FieldType.String, false, 10, "aaa", false)]
        [InlineData("2", "", FieldType.String, false, 10, "", false)]
        [InlineData("3", "333", FieldType.String, false, 10, "333", false)]
        [InlineData("4", null, FieldType.String, false, 10, null, false)]
        [InlineData("5", "   ", FieldType.String, true, 10, "   ", false)]
        [InlineData("6", "abcdefj", FieldType.String, true, 3, null, true)]
        public void ConvertFromStringToStringTests(string testDescription, string valueToTest, string dataType, bool allowEmptyField, int maxLength, string expected, bool throwsException)
        {
            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = FieldFormats.DefaultDateFormat,
                AllowEmptyField = allowEmptyField,
                AllowedMaximumLength = maxLength
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertFromString(valueToTest,
                new Mock<IReader>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expected);
            }
        }

        [Theory]
        [InlineData("1", "aaa", FieldType.String, false, 10, "aaa", false)]
        [InlineData("2", "", FieldType.String, false, 10, "", false)]
        [InlineData("3", "333", FieldType.String, false, 10, "333", false)]
        [InlineData("4", null, FieldType.String, false, 10, null, false)]
        [InlineData("5", "   ", FieldType.String, true, 10, "   ", false)]
        [InlineData("6", "abcdefj", FieldType.String, true, 3, null, true)]
        public void ConvertToStringFromStringTests(string testDescription, string valueToTest, string dataType, bool allowEmptyField, int maxLength, string expected, bool throwsException)
        {
            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = FieldFormats.DefaultDateFormat,
                AllowEmptyField = allowEmptyField,
                AllowedMaximumLength = maxLength
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertToString(valueToTest,
                new Mock<IWriterRow>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expected);
            }
        }

        [Theory]
        [InlineData("1", "0", FieldType.SignedNumber, false, 0, false)]
        [InlineData("2", "-111", FieldType.SignedNumber, false, -111, false)]
        [InlineData("3", "88788", FieldType.SignedNumber, false, 88788, false)]
        [InlineData("4", "2147483647", FieldType.SignedNumber, false, 2147483647, false)]
        [InlineData("5", "-2147483647", FieldType.SignedNumber, false, -2147483647, false)]
        [InlineData("6", "9223372036854775807", FieldType.SignedNumber, false, long.MaxValue, false)]
        [InlineData("7", "-9223372036854775808", FieldType.SignedNumber, false, long.MinValue, false)]
        [InlineData("8", "   ", FieldType.SignedNumber, true, null, false)]
        [InlineData("9", "   ", FieldType.SignedNumber, false, null, true)]
        [InlineData("10", "afsdf", FieldType.SignedNumber, true, null, true)]
        [InlineData("11", "9223372036854775808", FieldType.SignedNumber, false, null, true)]
        [InlineData("12", "-9223372036854775809", FieldType.SignedNumber, false, null, true)]
        public void ConvertFromStringToSignedNumberTests(string testDescription, string valueToTest, string dataType, bool allowEmptyField, object? expected, bool throwsException)
        {
            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = FieldFormats.DefaultDateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertFromString(valueToTest,
                new Mock<IReader>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expected);
            }
        }


        [Theory]
        [InlineData("1", 0, FieldType.SignedNumber, false, "0", false)]
        [InlineData("2", -111, FieldType.SignedNumber, false, "-111", false)]
        [InlineData("3", 88788, FieldType.SignedNumber, false, "88788", false)]
        [InlineData("4", 2147483647, FieldType.SignedNumber, false, "2147483647", false)]
        [InlineData("5", -2147483647, FieldType.SignedNumber, false, "-2147483647", false)]
        [InlineData("6", long.MaxValue, FieldType.SignedNumber, false, "9223372036854775807", false)]
        [InlineData("7", long.MinValue, FieldType.SignedNumber, false, "-9223372036854775808", false)]
        [InlineData("8", null, FieldType.SignedNumber, true, null, false)]
        public void ConvertToStringFromSignedNumberTests(string testDescription, object? valueToTest, string dataType, bool allowEmptyField, string expected, bool throwsException)
        {
            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = FieldFormats.DefaultDateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertToString(valueToTest,
                new Mock<IWriterRow>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expected);
            }
        }


        [Theory]
        [InlineData("1", "0", FieldType.UnsignedNumber, false, 0U, false)]
        [InlineData("2", "-111", FieldType.UnsignedNumber, false, null, true)]
        [InlineData("3", "88788", FieldType.UnsignedNumber, false, 88788U, false)]
        [InlineData("4", "2147483647", FieldType.UnsignedNumber, false, 2147483647U, false)]
        [InlineData("5", "-2147483647", FieldType.UnsignedNumber, false, null, true)]
        [InlineData("6", "9223372036854775807", FieldType.UnsignedNumber, false, 9223372036854775807U, false)]
        [InlineData("7", "-9223372036854775808", FieldType.UnsignedNumber, false, null, true)]
        [InlineData("8", "   ", FieldType.UnsignedNumber, true, null, false)]
        [InlineData("9", "   ", FieldType.UnsignedNumber, false, null, true)]
        [InlineData("10", "afsdf", FieldType.UnsignedNumber, true, null, true)]
        [InlineData("11", "9223372036854775808", FieldType.UnsignedNumber, false, 9223372036854775808U, false)]
        [InlineData("12", "-9223372036854775809", FieldType.UnsignedNumber, false, null, true)]
        [InlineData("13", "18446744073709551615", FieldType.UnsignedNumber, false, ulong.MaxValue, false)]  // max ulong
        [InlineData("14", "18446744073709551616", FieldType.UnsignedNumber, false, ulong.MaxValue, true)]   // max ulong plus 1
        public void ConvertFromStringToUnsignedNumberTests(string testDescription, string valueToTest, string dataType, bool allowEmptyField, object? expected, bool throwsException)
        {
            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = FieldFormats.DefaultDateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertFromString(valueToTest,
                new Mock<IReader>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expected);
            }
        }


        [Theory]
        [InlineData("1", 0U, FieldType.UnsignedNumber, false, "0", false)]
        [InlineData("3", 88788U, FieldType.UnsignedNumber, false, "88788", false)]
        [InlineData("4", 2147483647U, FieldType.UnsignedNumber, false, "2147483647", false)]
        [InlineData("6", ulong.MaxValue, FieldType.UnsignedNumber, false, "18446744073709551615", false)]
        [InlineData("8", null, FieldType.UnsignedNumber, true, null, false)]
        public void ConvertToStringFromUnsignedNumberTests(string testDescription, object? valueToTest, string dataType, bool allowEmptyField, string expected, bool throwsException)
        {
            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = FieldFormats.DefaultDateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertToString(valueToTest,
                new Mock<IWriterRow>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expected);
            }
        }


        [Theory]
        [InlineData("1", "1", "0", FieldType.Decimal, false, false)]
        [InlineData("2", "2", "-111", FieldType.Decimal, false, false)]
        [InlineData("3", "3", "88788", FieldType.Decimal, false, false)]
        [InlineData("4", "4", "2147483647", FieldType.Decimal, false, false)]
        [InlineData("5", "5", "-2147483647", FieldType.Decimal, false, false)]
        [InlineData("6", "6", "9223372036854775807", FieldType.Decimal, false, false)]
        [InlineData("7", "7", "-9223372036854775808", FieldType.Decimal, false, false)]
        [InlineData("8", "8", "9223372036854775807.89", FieldType.Decimal, false, false)]
        [InlineData("9", "9", "-9223372036854775808.89", FieldType.Decimal, false, false)]
        [InlineData("10", "10", "   ", FieldType.Decimal, true, false)]
        [InlineData("11", "11", "   ", FieldType.Decimal, false, true)]
        [InlineData("12", "12", "afsdf", FieldType.Decimal, true, true)]
        [InlineData("13", "13", "9223372036854775808", FieldType.Decimal, false, false)]
        [InlineData("14", "14", "-9223372036854775809", FieldType.Decimal, false, false)]
        [InlineData("15", "15", "79228162514264337593543950335", FieldType.Decimal, false, false)]
        [InlineData("16", "16", "-79228162514264337593543950335", FieldType.Decimal, false, false)]
        [InlineData("17", "17", "79228162514264337593543950336", FieldType.Decimal, false, true)]
        [InlineData("18", "18", "-79228162514264337593543950336", FieldType.Decimal, false, true)]
        [InlineData("19", "19", "792281625.123456789012345678901234567890123456789", FieldType.Decimal, false, false)]
        [InlineData("20", "20", "-792281625.123456789012345678901234567890123456789", FieldType.Decimal, false, false)]
        [InlineData("21", "21", "792281625.199", FieldType.Decimal, false, false)]
        [InlineData("22", "22", "-792281625.199", FieldType.Decimal, false, false)]
        [InlineData("23", "23", "-0.9999999999999999999999999999", FieldType.Decimal, false, false)]
        [InlineData("24", "24", "0.9999999999999999999999999999", FieldType.Decimal, false, false)]
        [InlineData("25", "25", "-0.99999999999999999999999999999", FieldType.Decimal, false,  false)]
        [InlineData("26", "26", "0.99999999999999999999999999999", FieldType.Decimal, false, false)]
        public void ConvertFromStringToDecimalTests(string testDescription, string expectedResultKey, string valueToTest, string dataType, bool allowEmptyField, bool throwsException)
        {
            // Cannot pass (compilation error) decimal as a parameter as inlinedata - so using this dictionary as a lookup with expectedResultKey
            var expectedResult = new Dictionary<string, object?>
            {
                {  "1",  0.00m },
                {  "2",  -111m },
                {  "3",  88788m },
                {  "4",  2147483647m },
                {  "5",  -2147483647m },
                {  "6",  9223372036854775807m },
                {  "7",  -9223372036854775808m },
                {  "8",  9223372036854775807.89m },
                {  "9",  -9223372036854775808.89m },
                {  "10", null },
                {  "11", null },
                {  "12",  null },
                {  "13",  9223372036854775808m },
                {  "14",  -9223372036854775809m },
                {  "15",  79228162514264337593543950335m },
                {  "16",  -79228162514264337593543950335M },
                {  "17",  null },
                {  "18",  null },
                {  "19",  792281625.123456789012345678901234567890123456789M },
                {  "20",  -792281625.123456789012345678901234567890123456789M },
                {  "21",  792281625.199M },
                {  "22",  -792281625.199M },
                {  "23",  -0.9999999999999999999999999999M },
                {  "24",  0.9999999999999999999999999999M },
                {  "25",  -1.0M },
                {  "26",  1.0M },
            };
            //NumberFormatInfo setPrecision = new NumberFormatInfo();

            //setPrecision.NumberDecimalDigits = 2;
            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = FieldFormats.DefaultDateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertFromString(valueToTest,
                new Mock<IReader>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expectedResult[expectedResultKey]);
            }
        }


        [Theory]
        [InlineData("1", "1", FieldType.Decimal, false, "0", false)]
        [InlineData("2", "2", FieldType.Decimal, false, "88788", false)]
        [InlineData("3", "3", FieldType.Decimal, false, "2147483647", false)]
        [InlineData("4", "4", FieldType.Decimal, false, "9223372036854775807", false)]
        [InlineData("5", "5", FieldType.Decimal, false, "-9223372036854775807", false)]
        [InlineData("6", "6", FieldType.Decimal, true, null, false)]
        [InlineData("7", "7", FieldType.Decimal, false, "-9223372036854775807.747446", false)]
        [InlineData("8", "8", FieldType.Decimal, false, "79228162514264337593543950335", false)]
        [InlineData("9", "9", FieldType.Decimal, false, "-79228162514264337593543950335", false)]
        [InlineData("10", "10", FieldType.Decimal, false, "79228162514264337593543950336", true)]
        [InlineData("11", "11", FieldType.Decimal, false, "-79228162514264337593543950336", true)]
        [InlineData("12", "12", FieldType.Decimal, false, "79228162514264337593543950335", false)]
        [InlineData("13", "13", FieldType.Decimal, false, "-79228162514264337593543950335", false)]
        [InlineData("14", "14", FieldType.Decimal, false, "792281625.199", false)]
        [InlineData("15", "15", FieldType.Decimal, false, "-792281625.199", false)]
        [InlineData("16", "16", FieldType.Decimal, false, "-0.79228162514264337593543999", false)]
        [InlineData("17", "17", FieldType.Decimal, false, "0.79228162514264337593543999", false)]
        [InlineData("18", "18", FieldType.Decimal, false, "-0.9999999999999999999999999999", false)]
        [InlineData("19", "19", FieldType.Decimal, false, "0.9999999999999999999999999999", false)]
        [InlineData("20", "20", FieldType.Decimal, false, "-1.0000000000000000000000000000", false)]
        [InlineData("21", "21", FieldType.Decimal, false, "1.0000000000000000000000000000", false)]
        public void ConvertToStringFromDecimalTests(string testDescription, string inputKey, string dataType, bool allowEmptyField, string expected, bool throwsException)
        {
            // Cannot pass (compilation error) decimal as a parameter as inlinedata - so using this dictionary as a lookup with inputKey
            var valueToTest = new Dictionary<string, object>
            {
                {  "1",  0m },
                {  "2",  88788m },
                {  "3",  2147483647m },
                {  "4",  9223372036854775807m },
                {  "5",  -9223372036854775807m },
                {  "6", null },
                {  "7",  -9223372036854775807.747446m },
                {  "8",  79228162514264337593543950335m },
                {  "9",  -79228162514264337593543950335M },
                {  "10",  null },
                {  "11",  null },
                {  "12",  decimal.MaxValue},
                {  "13",  decimal.MinValue },
                {  "14",  792281625.199M },
                {  "15",  -792281625.199M },
                {  "16",  -0.79228162514264337593543999M },
                {  "17",  0.79228162514264337593543999M },
                {  "18",  -0.9999999999999999999999999999M }, // 29 digit precision max supported by a decimal
                {  "19",  0.9999999999999999999999999999M },  // 29 digit precision max supported by a decimal
                {  "20",  -0.99999999999999999999999999999M }, // 30 digit precision 1 more than max supported by a decimal - result: rounded-off -1.0
                {  "21",  0.99999999999999999999999999999M },  // 30 digit precision 1 more than max supported by a decima - result: rounded-off to 1.0
            };
            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = FieldFormats.DefaultDateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertToString(valueToTest[inputKey],
                new Mock<IWriterRow>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expected);
            }
        }


        [Theory]
        [InlineData("1", "1", "0", FieldFormats.DefaultDateFormat, FieldType.Date, false, true)]
        [InlineData("2", "2", "aaaaa", FieldFormats.DefaultDateFormat, FieldType.Date, false, true)]
        [InlineData("3", "3", "2021/10/21", FieldFormats.DefaultDateFormat, FieldType.Date, false, false)]
        [InlineData("4", "4", "2021/10/21", "aaa", FieldType.Date, false, true)]
        [InlineData("5", "5", " ", FieldFormats.DefaultDateFormat, FieldType.Date, true, false)]
        [InlineData("6", "6", null, FieldFormats.DefaultDateFormat, FieldType.Date, true, false)]
        [InlineData("7", "7", "2021/21/10", "yyyy/dd/MM", FieldType.Date, false, false)]
        [InlineData("8", "8", "21/10/2021", "dd/MM/yyyy", FieldType.Date, false, false)]
        [InlineData("9", "9", "9999/21/10", "yyyy/dd/MM", FieldType.Date, false, false)]
        [InlineData("10", "10", "2021/21", "yyyy/dd", FieldType.Date, false, false)]
        [InlineData("11", "11", "2021/21/10", "dd/MM/yyyy", FieldType.Date, false, false)]
        [InlineData("11", "11", "2021/21/10", "dd/MM/yyyy", FieldType.Date, false, false)]
        public void ConvertFromStringToDateTests(string testDescription, string expectedResultKey, string valueToTest, string dateFormat, string dataType, bool allowEmptyField, bool throwsException)
        {
            // Cannot pass (compilation error) decimal as a parameter as inlinedata - so using this dictionary as a lookup with expectedResultKey
            var expectedResult = new Dictionary<string, DateTimeOffset?>
            {
                {  "1",  null },
                {  "2",  null },
                {  "3",  new DateTimeOffset(2021, 10, 21, 0, 0, 0, TimeSpan.Zero) },
                {  "4",  null },
                {  "5",  null },
                {  "6",  null },
                {  "7",  new DateTimeOffset(2021, 10, 21, 0, 0, 0, TimeSpan.Zero) },
                {  "8",  new DateTimeOffset(2021, 10, 21, 0, 0, 0, TimeSpan.Zero) },
                {  "9",  new DateTimeOffset(9999, 10, 21, 0, 0, 0, TimeSpan.Zero) },
                {  "10", new DateTimeOffset(2021, 1, 21, 0, 0, 0, TimeSpan.Zero) },
                {  "11", null },
            };

            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = dateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertFromString(valueToTest,
                new Mock<IReader>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expectedResult[expectedResultKey]);
            }
        }


        [Theory]
        [InlineData("1", "1", FieldFormats.DefaultDateFormat, FieldType.Date, false, null, true)]
        [InlineData("3", "3", FieldFormats.DefaultDateFormat, FieldType.Date, false, "2021/10/21", false)]
        [InlineData("7", "7", "yyyy/dd/MM", FieldType.Date, false, "2021/21/10", false)]
        [InlineData("8", "8", "dd/MM/yyyy", FieldType.Date, false, "21/10/2021", false)]
        [InlineData("9", "9", "yyyy/dd/MM", FieldType.Date, false, "9999/21/10", false)]
        [InlineData("10", "10", "yyyy/dd", FieldType.Date, false, "2021/21", false)]
        [InlineData("11", "11", "yyyy/dd", FieldType.Date, false, "9999/31", false)]
        [InlineData("12", "12", "yyyy/dd/MM", FieldType.Date, false, "9999/31/12", false)]
        [InlineData("13", "13", "yyyy/dd", FieldType.Date, false, "0001/01", false)]
        [InlineData("14", "14", "yyyy/dd/MM", FieldType.Date, false, "0001/01/01", false)]
        public void ConvertToStringFromDateTests(string testDescription, string valueToTestKey, string dateFormat, string dataType, bool allowEmptyField, string expectedResult, bool throwsException)
        {
            // Cannot pass (compilation error) decimal as a parameter as inlinedata - so using this dictionary as a lookup with expectedResultKey
            var valueToTest = new Dictionary<string, DateTimeOffset?>
            {
                {  "1",  null },
                {  "3",  new DateTimeOffset(2021, 10, 21, 0, 0, 0, TimeSpan.Zero) },
                {  "7",  new DateTimeOffset(2021, 10, 21, 0, 0, 0, TimeSpan.Zero) },
                {  "8",  new DateTimeOffset(2021, 10, 21, 0, 0, 0, TimeSpan.Zero) },
                {  "9",  new DateTimeOffset(9999, 10, 21, 0, 0, 0, TimeSpan.Zero) },
                {  "10", new DateTimeOffset(2021, 1, 21, 0, 0, 0, TimeSpan.Zero) },
                {  "11", new DateTimeOffset(DateTime.MaxValue, TimeSpan.Zero) },
                {  "12", new DateTimeOffset(DateTime.MaxValue, TimeSpan.Zero) },
                {  "13", new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero) },
                {  "14", new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero) },
            };

            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = dateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertToString(valueToTest[valueToTestKey],
                new Mock<IWriterRow>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expectedResult);
            }
        }


        [Theory]
        [InlineData("1", "1", "0", FieldFormats.DefaultTimeFormat, FieldType.Time, false, true)]
        [InlineData("2", "2", "aaaaa", FieldFormats.DefaultTimeFormat, FieldType.Time, false, true)]
        [InlineData("3", "3", "13:19", FieldFormats.DefaultTimeFormat, FieldType.Time, false, false)]
        [InlineData("4", "4", "13:19", "aaa", FieldType.Time, false, true)]
        [InlineData("5", "5", " ", FieldFormats.DefaultTimeFormat, FieldType.Time, true, false)]
        [InlineData("6", "6", null, FieldFormats.DefaultTimeFormat, FieldType.Time, true, false)]
        [InlineData("7", "7", "19:13", "mm:HH", FieldType.Time, false, true)]
        [InlineData("8", "8", "24:00", "HH:mm", FieldType.Time, false, true)]
        [InlineData("9", "9", "19:13", "hh\\:mm", FieldType.Time, false, false)]
        [InlineData("10", "10", "19:13", "mm\\:hh", FieldType.Time, false, false)]
        [InlineData("11", "11", "24:00", "HH\\:mm", FieldType.Time, false, true)]
        [InlineData("12", "12", "24:10", "HH\\:mm", FieldType.Time, false, true)]
        [InlineData("13", "13", "-4:10", "HH\\:mm", FieldType.Time, false, true)]
        public void ConvertFromStringToTimeTests(string testDescription, string expectedResultKey, string valueToTest, string dateFormat, string dataType, bool allowEmptyField, bool throwsException)
        {
            // Cannot pass (compilation error) decimal as a parameter as inlinedata - so using this dictionary as a lookup with expectedResultKey
            var expectedResult = new Dictionary<string, TimeSpan?>
            {
                {  "1",  new TimeSpan(13, 19, 0) },
                {  "2",  new TimeSpan(13, 19, 0) },
                {  "3",  new TimeSpan(13, 19, 0) },
                {  "4",  new TimeSpan(13, 19, 0) },
                {  "5",  null },
                {  "6",  null },
                {  "7",  new TimeSpan(13, 19, 0) },
                {  "8",  new TimeSpan(13, 19, 0) },
                {  "9",  new TimeSpan(19, 13, 0) },
                {  "10",  new TimeSpan(13, 19, 0) },
                {  "11",  new TimeSpan(13, 19, 0) },
                {  "12",  new TimeSpan(13, 19, 0) },
                {  "13",  new TimeSpan(13, 19, 0) },
            };

            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = dateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertFromString(valueToTest,
                new Mock<IReader>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expectedResult[expectedResultKey]);
            }
        }


        [Theory]
        [InlineData("1", "1", FieldFormats.DefaultTimeFormat, FieldType.Time, false, null, true)]
        [InlineData("3", "3", FieldFormats.DefaultTimeFormat, FieldType.Time, false, "13:19", false)]
        [InlineData("4", "4", "aaa", FieldType.Time, false, "13:19", true)]
        [InlineData("7", "7", "mm:HH", FieldType.Time, false, "19:13", true)]
        [InlineData("8", "8", "HH:mm", FieldType.Time, false, "24:00", true)]
        [InlineData("9", "9", "hh\\:mm", FieldType.Time, false, "19:13", false)]
        [InlineData("10", "10", "mm\\:hh", FieldType.Time, false, "19:13", false)]
        public void ConvertToStringFromTimeTests(string testDescription, string valueToTestKey, string dateFormat, string dataType, bool allowEmptyField, string expectedResult, bool throwsException)
        {
            // Cannot pass (compilation error) decimal as a parameter as inlinedata - so using this dictionary as a lookup with expectedResultKey
            var valueToTest = new Dictionary<string, TimeSpan?>
            {
                {  "1",  null },
                {  "3",  new TimeSpan(13, 19, 0) },
                {  "4",  null },
                {  "7",  null },
                {  "8",  null },
                {  "9",  new TimeSpan(19, 13, 0) },
                {  "10",  new TimeSpan(13, 19, 0) }
            };

            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = dateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertToString(valueToTest[valueToTestKey],
                new Mock<IWriterRow>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expectedResult);
            }
        }


        [Theory]
        [InlineData("1", "1", "0", FieldFormats.DefaultDateTimeFormat, FieldType.DateTime, false, true)]
        [InlineData("2", "2", "aaaaa", FieldFormats.DefaultDateTimeFormat, FieldType.DateTime, false, true)]
        [InlineData("3", "3", "2021/10/21 13:59", FieldFormats.DefaultDateTimeFormat, FieldType.DateTime, false, false)]
        [InlineData("4", "4", "2021/10/21 13:59", "aaa", FieldType.DateTime, false, true)]
        [InlineData("5", "5", " ", FieldFormats.DefaultDateTimeFormat, FieldType.DateTime, true, false)]
        [InlineData("6", "6", null, FieldFormats.DefaultDateTimeFormat, FieldType.DateTime, true, false)]
        [InlineData("7", "7", "2021/21/10 13:59", "yyyy/dd/MM HH:mm", FieldType.DateTime, false, false)]
        [InlineData("8", "8", "21/10/2021 13:59", "dd/MM/yyyy HH:mm", FieldType.DateTime, false, false)]
        [InlineData("9", "9", "9999/21/10 13:59", "yyyy/dd/MM HH:mm", FieldType.DateTime, false, false)]
        [InlineData("10", "10", "2021/21 13:59", "yyyy/dd HH:mm", FieldType.DateTime, false, false)]
        [InlineData("11", "11", "2021/21/10 13:59", "dd/MM/yyyy HH:mm", FieldType.DateTime, false, false)]
        public void ConvertFromStringToDateTimeTests(string testDescription, string expectedResultKey, string valueToTest, string dateFormat, string dataType, bool allowEmptyField, bool throwsException)
        {
            // Cannot pass (compilation error) decimal as a parameter as inlinedata - so using this dictionary as a lookup with expectedResultKey
            var expectedResult = new Dictionary<string, DateTimeOffset?>
            {
                {  "1",  null },
                {  "2",  null },
                {  "3",  new DateTimeOffset(2021, 10, 21, 13, 59, 0, TimeSpan.Zero) },
                {  "4",  null },
                {  "5",  null },
                {  "6",  null },
                {  "7",  new DateTimeOffset(2021, 10, 21, 13, 59, 0, TimeSpan.Zero) },
                {  "8",  new DateTimeOffset(2021, 10, 21, 13, 59, 0, TimeSpan.Zero) },
                {  "9",  new DateTimeOffset(9999, 10, 21, 13, 59, 0, TimeSpan.Zero) },
                {  "10", new DateTimeOffset(2021, 1, 21, 13, 59, 0, TimeSpan.Zero) },
                {  "11", null },
            };

            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = dateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertFromString(valueToTest,
                new Mock<IReader>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expectedResult[expectedResultKey]);
            }
        }


        [Theory]
        [InlineData("1", "1", FieldFormats.DefaultDateTimeFormat, FieldType.DateTime, false, null, true)]
        [InlineData("3", "3", FieldFormats.DefaultDateTimeFormat, FieldType.DateTime, false, "2021/10/21 13:19", false)]
        [InlineData("7", "7", "yyyy/dd/MM HH:mm", FieldType.DateTime, false, "2021/21/10 13:19", false)]
        [InlineData("8", "8", "dd/MM/yyyy HH:mm", FieldType.DateTime, false, "21/10/2021 13:19", false)]
        [InlineData("9", "9", "yyyy/dd/MM HH:mm", FieldType.DateTime, false, "9999/21/10 13:19", false)]
        [InlineData("10", "10", "yyyy/dd HH:mm", FieldType.DateTime, false, "2021/21 13:19", false)]
        [InlineData("11", "11", "yyyy/dd HH:mm", FieldType.DateTime, false, "9999/31 23:59", false)]
        [InlineData("12", "12", "yyyy/dd/MM HH:mm", FieldType.DateTime, false, "9999/31/12 23:59", false)]
        [InlineData("13", "13", "yyyy/dd HH:mm", FieldType.DateTime, false, "0001/01 00:00", false)]
        [InlineData("14", "14", "yyyy/dd/MM HH:mm", FieldType.DateTime, false, "0001/01/01 00:00", false)]
        public void ConvertToStringFromDateTimeTests(string testDescription, string valueToTestKey, string dateFormat, string dataType, bool allowEmptyField, string expectedResult, bool throwsException)
        {
            // Cannot pass (compilation error) decimal as a parameter as inlinedata - so using this dictionary as a lookup with expectedResultKey
            var valueToTest = new Dictionary<string, DateTimeOffset?>
            {
                {  "1",  null },
                {  "3",  new DateTimeOffset(2021, 10, 21, 13, 19, 0, TimeSpan.Zero) },
                {  "7",  new DateTimeOffset(2021, 10, 21, 13, 19, 0, TimeSpan.Zero) },
                {  "8",  new DateTimeOffset(2021, 10, 21, 13, 19, 0, TimeSpan.Zero) },
                {  "9",  new DateTimeOffset(9999, 10, 21, 13, 19, 0, TimeSpan.Zero) },
                {  "10", new DateTimeOffset(2021, 1, 21, 13, 19, 0, TimeSpan.Zero) },
                {  "11", new DateTimeOffset(DateTime.MaxValue, TimeSpan.Zero) },
                {  "12", new DateTimeOffset(DateTime.MaxValue, TimeSpan.Zero) },
                {  "13", new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero) },
                {  "14", new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero) },
            };

            //
            // Setup
            //
            var profile = new FieldConfiguration
            {
                Name = "Test",
                DataType = dataType,
                DateTimeFormat = dateFormat,
                AllowEmptyField = allowEmptyField
            };
            var converter = new FieldTypeConverter(profile);

            //
            // Act
            //
            object result = null;
            var exception = Xunit.Record.Exception(() =>
               result = converter.ConvertToString(valueToTest[valueToTestKey],
                new Mock<IWriterRow>().Object,
                new MemberMapData(this.GetType().GetMember(nameof(ConvertFromStringToStringTests)).First())));

            //
            // Verify
            //
            if (throwsException)
            {
                Assert.NotNull(exception);
                Assert.IsType<FieldTypeConverterException>(exception);
            }
            else
            {
                result.Should().Be(expectedResult);
            }
        }
    }
}
