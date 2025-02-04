using System.Threading;
using System.Threading.Tasks;

namespace UTG.StoreAndForward
{
    public interface IOfflineTransactionProcessor
    {
        Task ProcessOfflineTransactions(CancellationToken cancellationToken);
    }
}
