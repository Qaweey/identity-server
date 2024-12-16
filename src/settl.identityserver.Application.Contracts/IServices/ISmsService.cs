using settl.identityserver.Application.Contracts.DTO.OTP;
using settl.identityserver.Application.Contracts.DTO.SMS;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface ISmsService
    {
        Task<(bool, BaseSettlApiDTO)> SendSMS(SMSRequest request);
    }
}