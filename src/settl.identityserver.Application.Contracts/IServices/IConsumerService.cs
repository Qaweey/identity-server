using settl.identityserver.Application.Contracts.DTO.Consumer;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Domain.Shared;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface IConsumerService
    {
        Task<(bool, CreateConsumerResponseDTO)> Create(CreateConsumerDTO model);

        Task<(bool, ConsumerProfileResponseDTO)> GetProfile(string phone, string token);

        public Task<ResponsesDTO> RegisterNewDevice(RegisterNewDeviceDto newDeviceDto);

        public Task<string> VerifyPinAndSendOTP(string phone, string transPin);
    }
}