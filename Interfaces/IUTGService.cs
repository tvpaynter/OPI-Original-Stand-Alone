using System.Threading;
using System.Threading.Tasks;
using UTG.Models.OPIModels;

namespace UTG.Interfaces
{
    public interface IUTGService
    {
        Task<TransactionResponse> ProcessMessage(TransactionRequest request, CancellationToken cancellationToken = default);

        Task<TokenResponse> ProcessTokenMessage(TokenRequest request, CancellationToken cancellationToken = default);
    }
}