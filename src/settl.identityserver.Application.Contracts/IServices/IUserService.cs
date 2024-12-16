using settl.identityserver.Application.Contracts.DTO;
using settl.identityserver.Application.Contracts.DTO.Admin;
using settl.identityserver.Application.Contracts.DTO.Consumer;
using settl.identityserver.Application.Contracts.DTO.Referral;
using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Helpers.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface IUserService
    {
        public Task<ResponsesDTO> SignUpUser(CreateUsersDTO createConsumerDTO);

        public Task<IEnumerable<UserDTO>> Get();

        Task<Auth> GetUser(string phone);

        public Task<(bool, string)> FreezeWallet(FreezeWalletDTO request);

        public Task<(bool, string)> BlacklistDevice(BlacklistDeviceDTO request);

        public Task<UserDTO> Get(string phone);

        public Task<ResponsesDTO> VerifyConsumerPassword(VerifyConsumerPasswordDTO verifyConsumerPasswordDTO);

        Task<(ResponsesDTO, UsersDTO, RefreshTokenRequest)> SignInConsumer(SignInConsumerDTO signInConsumerDTO);

        public Task<bool> SignOutConsumer(string phone);

        public void SignOut(string phone);

        public Task<(ConsumerWalletDTO, List<ConsumerWallet>)> GetConsumerWallets(string phoneNumber, string token);

        public Task<ResponsesDTO> GetAgencyWallet(string phoneNumber);

        public Task<(bool, PhoneOSAndFcmTokenModel)> GetFcmTokenAndPhoneOS(string phone);

        public Task<(bool, string)> UpdateFcmToken(UpdateFcmTokenDTO model);

        public Task<ResponsesDTO> ResetConsumerPassword(ResetConsumerPasswordDTO resetConsumerPasswordDTO);

        public Task<ResponsesDTO> ChangePassword(ChangePasswordDTO changeConsumerPasswordDTO);

        public Task<ResponsesDTO> CreateConsumerSecurityAnswer(List<CreateSecurityAnswerDTO> createSecurityAnswerDTO);

        public Task<ResponsesDTO> VerifyConsumerReferralCode(VerifyReferralCodeDTO verifyReferralCodeDTO);

        Task<UsersDateFilterResponse> GetUsersFilter(int? days, string start, string end);

        Task<bool> IsBlacklistedNumber(string phone);

        Task<bool> PhoneExists(CreateConsumerDTO consumer);

        Task<AppDownloads> AppDownloads(int days, string startDate, string endDate);
    }
}