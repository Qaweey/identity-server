using settl.identityserver.Application.Contracts.DTO.OTP;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Domain.Shared;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface IOTPService
    {
        public Task<bool> SendOTP(SendSMSDTO sendSMSDTO);

        public Task<ResponsesDTO> VerifyOTP(VerifyOTPDTO verifyOTPDTO);

        public Task<(bool, string)> Verify(string otp, string phone, string email = "");
    }
}