using System.Collections.Generic;
using System.Threading.Tasks;
using UTG.Models;
using UTG.Models.OPIModels;
using UTG.Models.TerminalModels;

namespace UTG.Data
{
    public interface IOfflineDBManager
    {
        void SaveOfflineTransaction(OfflineTransactionModel offlineTransactionModel);
        Task<IEnumerable<OfflineTransactionModel>> GetAllOfflineTransactions();
        Task<IEnumerable<OfflineTransactionModel>> GetOfflineTransactions(int status);
        Task<OfflineTransactionModel> GetOfflineTransacionByTransToken(string transToken);
        void UpdateOfflineTransactionAsync(TransactionResponse opiresponse,int id,byte[] IV);
        string GetEncryptionKey();
        Task ProcessOfflineRequest();
        Task DeleteProcessedOfflineTransaction(TokenRequestCardInfo[] tokenRequestCardInfos);
        void UpdateOfflineTerminalTransaction(m4 m4Response, OfflineTransactionModel offlineTransactionModel);
        Task<OfflineTransactionModel> GetOfflineTransacionByPurchaseId(string purchaseId);
    }
}
