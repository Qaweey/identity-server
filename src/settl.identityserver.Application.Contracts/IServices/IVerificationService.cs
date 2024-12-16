using settl.identityserver.Application.Contracts.DTO.KYC;
using settl.identityserver.Domain.Shared.Helpers.ApiConnect;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface IVerificationService 
    {
        Task<(bool, dynamic)> VerifyKYC(KycDTO kyc);
     
        Task<(bool, dynamic)> VerifySelfie(Welcome2 uploadselfie);
        (string, string) GenerateSecCredentials(string api_key, string partnerId);
        Task<bool> UpdateSelfie(SelfieKycUploadResponseDTO selfie);
    }
}
