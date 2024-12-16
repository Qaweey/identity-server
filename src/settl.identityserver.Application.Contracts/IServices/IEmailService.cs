using settl.identityserver.Application.Contracts.DTO;
using settl.identityserver.Application.Contracts.DTO.SMS;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface IEmailService
    {
        Task<(bool, BaseSettlApiDTO)> SendSettlEmail(EmailRequest request);

        Task<EmailTemplate> GetTemplate(string code);

        Task<(bool, List<BaseSettlApiDTO>)> SendBulkSettlEmail(BulkEmailRequest request);

        Task<bool> SendRegistrationEmail(UserDTO user, bool isReferred);

        Task<(bool, List<BaseSettlApiDTO>)> SendBulkTemplateEmail(AWSBulkEmailDTO request);

        Task<(bool, BaseSettlApiDTO)> SendSettlEmail(EmailRequestTemplate request);
    }
}