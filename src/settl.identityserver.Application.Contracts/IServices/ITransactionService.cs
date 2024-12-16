using settl.identityserver.Application.Contracts.DTO.Banking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface ITransactionService
    {
        Task<(List<TransactionDTO>, string message)> Get(string phone);
    }
}