using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UTG.Common;
using UTG.Common.Constants;
using UTG.Common.Handlers;
using UTG.Data;
using UTG.Interfaces;
using UTG.Models;
using UTG.Models.OPIModels;

namespace UTG.Services
{
    /// <summary>
    /// Trx Service Class
    /// </summary>
    public class HostService : IUTGService
    {
        private readonly ILogger<HostService> _logger;
        private readonly HttpPostHandler _httpHandler;
        private readonly IConfiguration _configuration;
        private readonly IOfflineDBManager _offlineDBManager;
        /// <summary>
        /// TrxService Construtor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpHandler"></param>
        /// <param name="configuration"></param>
        public HostService(ILogger<HostService> logger, HttpPostHandler httpHandler, IConfiguration configuration, IOfflineDBManager offlineDBManager)
        {
            _logger = logger;
            _httpHandler = httpHandler;
            _configuration = configuration;
            _offlineDBManager = offlineDBManager;
        }
        /// <summary>
        /// Process OPI request  
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TransactionResponse> ProcessMessage(TransactionRequest request, CancellationToken cancellationToken = default)
        {
            var TempTransactionType = request.TransType;
            if (request.TransType == OPITransactionType.PreAuth && !string.IsNullOrEmpty(request.TransToken))
            {
                request.TransType = OPITransactionType.IncrementalAuth;
            }
            if (request.TransType == OPITransactionType.Refund)
            {
                request.TransType = OPITransactionType.AuthRelease;
            }
            var response = await _httpHandler.SendMessage(request);
            _ = _offlineDBManager.ProcessOfflineRequest().ConfigureAwait(false);
            var Opiresponse = JsonConvert.DeserializeObject<TransactionResponse>(response);
            Opiresponse.TransType = TempTransactionType;
            _logger.LogInformation("Response to OPI : {@Opiresponse}", Opiresponse);

            return Opiresponse;
        }

        public async Task<TokenResponse> ProcessTokenMessage(TokenRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Request from OPI : {@request}", request);
            TokenResponse response = new();
            if (request.TransType == OPITransactionType.UpdateToken)
            {
                response = await GetUpdatedToken(request);
                if (response.CardInfo.Length > 0)
                {
                    _ = _offlineDBManager.DeleteProcessedOfflineTransaction(request.CardInfo).ConfigureAwait(false);
                }
            }
            else
            {
                response = await ProcessAndUpdateToken(request);
            }
            _logger.LogInformation("Response to OPI : {@response}", response);
            return response;
        }

        private async Task<TokenResponse> ProcessAndUpdateToken(TokenRequest request)
        {

            TokenResponse tokenResponse = new();
            tokenResponse.SequenceNo = request.SequenceNo;
            tokenResponse.TransType = request.TransType;
            tokenResponse.RespCode = UTGConstants.OPIApprovedRespCode;
            tokenResponse.RespText = UTGConstants.OPIApprovedRespText;
            tokenResponse.CardInfo = new TokenResponseCardInfo[request.CardInfo.Length];
            for (int i = 0; i < request.CardInfo.Length; i++)
            {

                TransactionRequest transRequest = buildmanualTransactionRequest(request.CardInfo[i]);
                transRequest.SequenceNo = request.SequenceNo;
                transRequest.SiteId = request.SiteId;
                transRequest.TransDateTime = DateTime.Parse(request.TransDateTime);
                transRequest.WSNo = "NA";
                transRequest.POSInfo = request.POSInfo;
                transRequest.IndustryCode = int.Parse(request.IndustryCode);
                var response = await _httpHandler.SendMessage(transRequest);
                var Opiresponse = JsonConvert.DeserializeObject<TransactionResponse>(response);
                
                if (Opiresponse.RespCode == UTGConstants.OPIApprovedRespCode)
                {
                    tokenResponse.CardInfo[i] = new TokenResponseCardInfo
                    {
                        Record = request.CardInfo[i].Record,
                        TransToken = Opiresponse.TransToken,
                        IssuerId = Opiresponse.IssuerId,
                        PAN = request.CardInfo[i].PAN.MaskedCardNumber().Replace(" ","").Trim()
                    };
                }
            }
            tokenResponse.CardInfo = tokenResponse.CardInfo.Where(c => c != null).ToArray();

            if (tokenResponse.CardInfo.Length == 0)
            {
                tokenResponse.RespCode = UTGConstants.OPINotFoundRespCode;
                tokenResponse.RespText = UTGConstants.OPINotFoundRespText;
            }
            else if (tokenResponse.CardInfo.Length != request.CardInfo.Length)
            {
                tokenResponse.RespCode = UTGConstants.OPIPartialApprovedRespCode;
                tokenResponse.RespText = UTGConstants.OPIPartialApprovedRespText;
            }
            return tokenResponse;

        }

        private TransactionRequest buildmanualTransactionRequest(TokenRequestCardInfo tokenRequestCardInfo)
        {
            TransactionRequest transactionRequest = new()
            {
                Pan = tokenRequestCardInfo.PAN,
                ExpiryDate = tokenRequestCardInfo.ExpiryDate,
                TransAmount = 100,
                TaxAmount = "0",
                PartialAuthFlag = 0,
                CardPresent = "1",
                TransType =OPITransactionType.PreAuth
            };
            return transactionRequest;
        }

        private async Task<TokenResponse> GetUpdatedToken(TokenRequest request)
        {
            TokenResponse tokenResponse = new();
            tokenResponse.SequenceNo = request.SequenceNo;
            tokenResponse.TransType = request.TransType;
            tokenResponse.RespCode = UTGConstants.OPIApprovedRespCode;
            tokenResponse.RespText = UTGConstants.OPIApprovedRespText;
            tokenResponse.CardInfo = new TokenResponseCardInfo[request.CardInfo.Length];
            for (int i = 0; i < request.CardInfo.Length; i++)
            {
                OfflineTransactionModel offlineTransactionModel = await _offlineDBManager.GetOfflineTransacionByTransToken(request.CardInfo[i].TransToken);

                if (offlineTransactionModel?.ResponseCode == UTGConstants.OPIApprovedRespCode)
                {
                    JsonResponseFields jsonResponseFields = JsonConvert.DeserializeObject<JsonResponseFields>(offlineTransactionModel.JsonResponseFields);
                    tokenResponse.CardInfo[i] = new TokenResponseCardInfo
                    {
                        Record = request.CardInfo[i].Record,
                        TransToken = jsonResponseFields.UpdatedTransToken,
                        IssuerId = jsonResponseFields.IssuerId,
                        PAN = jsonResponseFields.MaskedPan
                    };
                }
            }
            tokenResponse.CardInfo = tokenResponse.CardInfo.Where(c => c != null).ToArray();

            if (tokenResponse.CardInfo.Length == 0)
            {
                tokenResponse.RespCode = UTGConstants.OPINotFoundRespCode;
                tokenResponse.RespText = UTGConstants.OPINotFoundRespText;
            }
            else if (tokenResponse.CardInfo.Length != request.CardInfo.Length)
            {
                tokenResponse.RespCode = UTGConstants.OPIPartialApprovedRespCode;
                tokenResponse.RespText = UTGConstants.OPIPartialApprovedRespText;
            }
            return tokenResponse;
        }
    }

}
