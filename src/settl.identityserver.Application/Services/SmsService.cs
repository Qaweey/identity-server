using Serilog;
using settl.identityserver.Application.Contracts.DTO.OTP;
using settl.identityserver.Application.Contracts.DTO.SMS;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.EntityFrameworkCore.RepositoryImplementations;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class SmsService : SmsRepository<BaseSettlApiDTO>, ISmsService
    {
        public async Task<(bool, BaseSettlApiDTO)> SendSMS(SMSRequest request)
        {
            try
            {
                Connect();
                var url = "/sms/send";
                var (response, data) = await PostSMSAsync(request, url);

                if (response is null)
                {
                    throw new CustomException($"SMS response code - {data}");
                }

                if (response?.Code == "00") return (true, response);
                else throw new CustomException(response?.Message ?? response?.Errors.ToString());
            }
            catch (CustomException ex)
            {
                Log.Error(ex.Message);
                return (false, new BaseSettlApiDTO() { Message = ex.Message });
            }
        }
    }
}