using System;
using System.Net;

namespace Lekha.Uploader.Models
{
    public class ServiceException : Exception
    {
        public int Status { get; set; } = (int)HttpStatusCode.InternalServerError;
        public object? Value { get; set; }
        public ServiceException(string message)
            : base(message)
        {
        }
        public ServiceException(string message, object value)
            : base(message)
        {
            Value = value;
        }
        public ServiceException(string message, object value, Exception innerException)
            : base(message, innerException)
        {
            Value = value;
        }

        public override string ToString()
        {
            var valueStr = Value == null ? null : $"Data: {System.Text.Json.JsonSerializer.Serialize(Value)}";
            return $"{base.ToString()}{Environment.NewLine} {valueStr}";
        }
    }
}
