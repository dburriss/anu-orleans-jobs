using System;
using System.Collections.Generic;
using System.Text;
using Orleans;

namespace Anu.Jobs
{
    /// <summary>
    /// Captures essential exception information in a serialization-friendly format.
    /// </summary>
    [GenerateSerializer]
    public class ExceptionInfo
    {
        /// <summary>
        /// The type name of the exception.
        /// </summary>
        [Id(0)]
        public string ExceptionType { get; set; } = string.Empty;
        
        /// <summary>
        /// The exception message.
        /// </summary>
        [Id(1)]
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// The stack trace as a string.
        /// </summary>
        [Id(2)]
        public string StackTrace { get; set; } = string.Empty;
        
        /// <summary>
        /// Additional data from the exception's Data dictionary.
        /// </summary>
        [Id(3)]
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Information about inner exceptions, if any.
        /// </summary>
        [Id(4)]
        public ExceptionInfo? InnerException { get; set; }
        
        /// <summary>
        /// Creates an empty ExceptionInfo.
        /// </summary>
        public ExceptionInfo() { }
        
        /// <summary>
        /// Creates an ExceptionInfo from an Exception.
        /// </summary>
        public ExceptionInfo(Exception exception)
        {
            if (exception == null) return;
            
            ExceptionType = exception.GetType().FullName ?? "Unknown";
            Message = exception.Message;
            StackTrace = exception.StackTrace ?? string.Empty;
            
            // Capture exception data
            if (exception.Data != null)
            {
                foreach (var key in exception.Data.Keys)
                {
                    if (key != null && exception.Data[key] != null)
                    {
                        Data[key.ToString()!] = exception.Data[key]!.ToString()!;
                    }
                }
            }
            
            // Capture inner exception if present
            if (exception.InnerException != null)
            {
                InnerException = new ExceptionInfo(exception.InnerException);
            }
        }
        
        /// <summary>
        /// Returns a string representation of the exception.
        /// </summary>
        public override string ToString()
        {
            return $"{ExceptionType}: {Message}";
        }
        
        /// <summary>
        /// Creates a formatted string with the full exception details.
        /// </summary>
        public string ToDetailedString()
        {
            var sb = new StringBuilder();
            BuildExceptionString(sb, this, 0);
            return sb.ToString();
        }
        
        private void BuildExceptionString(StringBuilder sb, ExceptionInfo ex, int level)
        {
            string indent = new string(' ', level * 4);
            
            sb.AppendLine($"{indent}Exception: {ex.ExceptionType}");
            sb.AppendLine($"{indent}Message: {ex.Message}");
            
            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                sb.AppendLine($"{indent}StackTrace:");
                sb.AppendLine($"{indent}{ex.StackTrace}");
            }
            
            if (ex.Data.Count > 0)
            {
                sb.AppendLine($"{indent}Additional Data:");
                foreach (var kvp in ex.Data)
                {
                    sb.AppendLine($"{indent}    {kvp.Key}: {kvp.Value}");
                }
            }
            
            if (ex.InnerException != null)
            {
                sb.AppendLine($"{indent}Inner Exception:");
                BuildExceptionString(sb, ex.InnerException, level + 1);
            }
        }
    }
}
