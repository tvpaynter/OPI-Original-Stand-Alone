using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UTG.Common;
using UTG.Common.Constants;
using UTG.Common.LogHelper;
using UTG.Models.OPIModels;

namespace UTG.Exceptions
{
    /// <summary>
    /// Global Error Handling Middleware
    /// </summary>
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;
        private readonly string _serviceName;
        private static readonly string OPIRequest = "Request";
        private static readonly string OPIResponse = "Response";
        public static IConfiguration _configuration { get; set; }
        /// <summary>
        /// Global Error Handling Middleware
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="next"></param>
        /// <param name="serviceName"></param>
        /// <param name="configuration"></param>
        public GlobalErrorHandlingMiddleware(ILogger<GlobalErrorHandlingMiddleware> logger,RequestDelegate next, string serviceName, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
            _serviceName = serviceName;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var request = context.Request;
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry("GlobalErrorHandlingMiddleware 1 Content Type : " + context.Request.ContentType, EventLogEntryType.Information, 101, 1);
                }

                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                //get body string here...
                var requestContent = Encoding.UTF8.GetString(buffer);
                request.Body.Position = 0;  //rewinding the stream to 0
                context.Items.Add(OPIRequest, requestContent);
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            string bufferTransactionRequest = context.Items[OPIRequest]?.ToString();          
            TransactionRequest transactionRequest = Utils.DeserializeToObject<TransactionRequest>(bufferTransactionRequest);
            TransactionResponse transactionResponse = Utils.BuildErrorResponse(UTGConstants.OPIErrorRespCode, UTGConstants.OPIErrorRespText, transactionRequest);
            context.Response.ContentType = context.Request.ContentType;
            LogLevel logLevel = LogLevel.Error;
            switch (exception)
            {
                case OpiException:
                    transactionResponse.RespText = exception.Message;
                    break;
                case ConnectivityException:
                    transactionResponse.RespText = exception.Message;
                    transactionResponse.OfflineFlag = "Y";
                    context.Items.Add(OPIResponse, Utils.Serialize(transactionResponse));
                    await _next(context);
                    transactionResponse = Utils.DeserializeToObject<TransactionResponse>(context.Items[OPIResponse].ToString());                    
                    break;
            }
            _logger.LogErrorDetails(context, exception, _serviceName, _configuration, logLevel);
            _logger.LogInformation("Offline Response to OPI :{@transactionResponse}", transactionResponse);
            await context.Response.WriteAsync(Utils.Serialize(transactionResponse));

        }
    }
}
