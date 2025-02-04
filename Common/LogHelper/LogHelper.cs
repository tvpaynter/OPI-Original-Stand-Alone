using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UTG.Common.LogHelper
{
    internal static class LogHelper
    {
        public static void LogErrorDetails(this ILogger logger, HttpContext context, Exception exception,
            string serviceName, IConfiguration config, LogLevel logLevel)
        {
            try
            {
                var contextInfo = GetContextInfo(context, exception, serviceName, config);
                logger.Log(logLevel, $"{contextInfo}");
            }
            catch (Exception)
            {
                //supress exception
            }

        }

        private static string GetContextInfo(HttpContext context, Exception exception, string serviceName, IConfiguration config)
        {
            if (context?.Request == null)
            {
                return string.Empty;
            }
            var result = new StringBuilder();
            result.AppendLine($"Message: {exception.Message}'");
            return result.ToString();

        }
    }
}
