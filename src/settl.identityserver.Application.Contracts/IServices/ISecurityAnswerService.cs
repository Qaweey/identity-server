using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared.Helpers.Authentication;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface ISecurityAnswerService
    {
        public Task<(bool, RefreshTokenRequest)> CreateConsumerSecurityAnswer(CreateSecurityAnswerForm model);

        public Task<SecurityAnswer> GetSecurityAnswers(string phone);
    }
}