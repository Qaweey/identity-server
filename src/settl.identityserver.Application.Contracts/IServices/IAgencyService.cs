using settl.identityserver.Application.Contracts.DTO.Agency;
using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Domain.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface IAgencyService
    {
        public Task<ResponsesDTO> CreateAgency(CreateAgencyDTO createAgencyDTO);

        public Task<ResponsesDTO> SignInAgency(SignInAgencyDTO signInAgencyDTO);

        public Task<ResponsesDTO> UpdateAgencyPassword(UpdateAgencyPasswordDTO updateAgencyPasswordDTO);

        public Task<ResponsesDTO> UploadAgencyBusinessDocument(UploadBusinessDocumentDTO uploadBusinessDocumentDTO);

        public Task<ResponsesDTO> UploadAgencySelfie(UploadSelfieDTO uploadSelfieDTO);

        public Task<ResponsesDTO> CreateAgencySecurityAnswer(List<CreateSecurityAnswerDTO> createSecurityAnswerDTO);

        public Task<ResponsesDTO> VerifyAgencyNIN(VerifyAgencyNINDTO verifyAgencyNINDTO);
    }
}
