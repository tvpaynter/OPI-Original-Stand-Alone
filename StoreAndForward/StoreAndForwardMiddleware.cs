using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using UTG.Common;
using UTG.Common.Constants;
using UTG.Data;
using UTG.Models;
using UTG.Models.OPIModels;

namespace UTG.StoreAndForward
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project

    public class StoreAndForwardMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string OPIRequest = "Request";
        private static readonly string OPIResponse = "Response";
        private readonly IOfflineDBManager _offlineDBManager;
        private readonly ILogger<StoreAndForwardMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public StoreAndForwardMiddleware(RequestDelegate next, IOfflineDBManager offlineDBManager, ILogger<StoreAndForwardMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _offlineDBManager = offlineDBManager;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext)
        {

            //if(httpContext.Items.ContainsKey(OPIRequest))
            if (httpContext.Items.ContainsKey(OPIResponse))
            {
                TransactionResponse Opiresponse = Utils.DeserializeToObject<TransactionResponse>(httpContext.Items[OPIResponse].ToString());
                if (Opiresponse.OfflineFlag == "Y")
                    await StoreOfflineTransaction(httpContext).ConfigureAwait(false);
            }
            else
            {
                await _next(httpContext);
            }
        }

        private async Task StoreOfflineTransaction(HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Storing Offline Transaction.");
            byte[] IV = Encoding.ASCII.GetBytes(Utils.GetRandomIV());
            string bufferTransactionRequest = context.Items[OPIRequest]?.ToString();
            context.Response.ContentType = context.Request.ContentType;
            TransactionRequest transactionRequest = Utils.DeserializeToObject<TransactionRequest>(bufferTransactionRequest);
            TransactionResponse Opiresponse = Utils.DeserializeToObject<TransactionResponse>(context.Items[OPIResponse].ToString());

            OfflineTransactionModel offlineTransaction = new();
            offlineTransaction.TransactionType = Opiresponse.TransType;
            string formattedSequenceNumber = String.Format("{0:D12}", Int64.Parse(transactionRequest.SequenceNo));
            offlineTransaction.TransToken = Utils.GetTempTransToken(transactionRequest.SequenceNo, "OFF");
            _logger.Log(LogLevel.Information, $"Offline Transaction Token : {offlineTransaction.TransToken}");
            offlineTransaction.SequenceNo = Opiresponse.SequenceNo;
            if (transactionRequest.TransType == OPITransactionType.IsOnline)
            {
                Opiresponse.RespCode = UTGConstants.OPIApprovedRespCode;
                Opiresponse.RespText = UTGConstants.OPIIsOnlineOfflineRespText;
                Opiresponse.TransAmount = null;
                Opiresponse.RRN = "000000000000";
            }
            else
            {
                offlineTransaction.DateTime = DateTime.UtcNow.ToString();
                offlineTransaction.Status = (int)UTGConstants.OfflineTransactionStatus.Added;
                offlineTransaction.Request = Utils.EncryptPayload(JsonConvert.SerializeObject(transactionRequest), _offlineDBManager.GetEncryptionKey(), IV);
                offlineTransaction.IV = Convert.ToBase64String(IV);
                _logger.LogInformation("Storing Offline Transaction Request : {@transactionRequest}", transactionRequest);
                _offlineDBManager.SaveOfflineTransaction(offlineTransaction);
                _logger.Log(LogLevel.Debug, "Offline Transaction saved successfully");
                Opiresponse.RespCode = UTGConstants.OPIApprovedRespCode;
                Opiresponse.RespText = UTGConstants.OPIOfflineApprovedRespText;
                Opiresponse.TransToken = offlineTransaction.TransToken;
                Random random = new();
                Opiresponse.AuthCode = UTGConstants.OPIOfflineAuthCode + random.Next(100, 999);
                Opiresponse.RRN = string.IsNullOrEmpty(transactionRequest.OriginalRRN) ? formattedSequenceNumber : transactionRequest.OriginalRRN;
            }
            context.Items[OPIResponse] = Utils.Serialize(Opiresponse);
        }
    }
}
