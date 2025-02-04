using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Utg.Api.Validators;
using UTG.Common;
using UTG.Interfaces;
using UTG.Models.OPIModels;
using UTG.Services;
using UTG.StoreAndForward;
using UTG.Validators;

namespace UTG.Controllers
{
    [Route("v1/UTGService")]
    [ApiController]
    public class OPIController : ControllerBase
    {
        private readonly ILogger<OPIController> _logger;
        private readonly Func<string, string, string, IUTGService> _serviceProvider;

        /// <summary>
        /// OpiListenerController Construtor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="uTGServiceProvider"></param>
        public OPIController(ILogger<OPIController> logger, Func<string, string, string, IUTGService> serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// OPI Process request
        /// </summary>
        /// <param name="transRequest"></param>
        /// <returns></returns>
        [HttpPost("TransactionRequest")]
        public async Task<TransactionResponse> OPITransactionRequest(TransactionRequest transRequest)
        {
            _logger.LogInformation("OPI Transaction Request : {@transRequest}", transRequest);
            new RequestValidator(transRequest.TransType).ValidateModel(transRequest);
            IUTGService uTGServiceProvider = _serviceProvider(transRequest.TransToken, transRequest.Pan, transRequest.TransType);
            return await uTGServiceProvider.ProcessMessage(transRequest);
        }

        [HttpPost("TokenRequest")]
        public async Task<TokenResponse> OPITokenRequest(TokenRequest tokenRequest)
        {
            _logger.LogInformation("OPI Token Request : {@tokenRequest} " + tokenRequest);
            IUTGService uTGServiceProvider = _serviceProvider(null, null, tokenRequest.TransType);
            return await uTGServiceProvider.ProcessTokenMessage(tokenRequest);
        }
    }
}
