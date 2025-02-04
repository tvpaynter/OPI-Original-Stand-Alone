using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UTG.Common;
using UTG.Common.Constants;
using UTG.Common.Handlers;
using UTG.Data;
using UTG.Models.OPIModels;

namespace UTG.StoreAndForward
{
    public class OfflineTransactionProcessor : IOfflineTransactionProcessor
    {
        private readonly ILogger<OfflineTransactionProcessor> _logger;
        private readonly IConfiguration _configuration;
        private readonly IOfflineDBManager _offlineDBManager;
        private readonly HttpPostHandler _httpPostHandler;
        public OfflineTransactionProcessor(ILogger<OfflineTransactionProcessor> logger, IConfiguration configuration, IOfflineDBManager offlineDBManager, HttpPostHandler httpPostHandler)
        {
            _logger = logger;
            _configuration = configuration;
            _offlineDBManager = offlineDBManager;
            _httpPostHandler = httpPostHandler;
        }
        public async Task ProcessOfflineTransactions(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                if (Utils.IsOnline(_configuration["HostSettings:URL"]))
                {
                    await _offlineDBManager.ProcessOfflineRequest();
                }
                await Task.Delay(1000 * Convert.ToInt32(_configuration["SnFSettings:OfflineTransactionIntervalInMin"]) * 60, cancellationToken);

            }
        }
    }
}
