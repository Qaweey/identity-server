using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Application.Contracts.DTO.SecurityQuestion;
using settl.identityserver.Domain.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface ISecurityQuestionService
    {
        List<SecurityQuestionDTO> GetSecurityQuestion();

        Task<(bool, string)> VerifyAnswers(VerifyAnswerForm model);

        Task<ResponsesDTO> GetSelectedSecurityQuestion(string phone);
    }
}