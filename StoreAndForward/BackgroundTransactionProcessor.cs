using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace UTG.StoreAndForward
{
    public class BackgroundTransactionProcessor: IHostedService
    {
        private readonly IOfflineTransactionProcessor _offlineTransactionProcessor;
        public BackgroundTransactionProcessor(IOfflineTransactionProcessor offlineTransactionProcessor)
        {
            _offlineTransactionProcessor = offlineTransactionProcessor;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _offlineTransactionProcessor.ProcessOfflineTransactions(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
