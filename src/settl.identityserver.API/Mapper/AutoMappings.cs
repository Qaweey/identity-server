using AutoMapper;
using IdentityModel.Client;
using settl.identityserver.Application.Contracts.DTO.Admin;
using settl.identityserver.Application.Contracts.DTO.Agency;
using settl.identityserver.Application.Contracts.DTO.Consumer;
using settl.identityserver.Application.Contracts.DTO.KYC;
using settl.identityserver.Application.Contracts.DTO.Onboarding;
using settl.identityserver.Application.Contracts.DTO.OTP;
using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Application.Contracts.DTO.SecurityQuestion;
using settl.identityserver.Application.Contracts.DTO.TransactionPIN;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Domain.Entities;

namespace settl.identityserver.API.Mapper
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<Auth, UserDTO>().ReverseMap();
            CreateMap<Auth, CreateUsersDTO>().ReverseMap();
            CreateMap<Auth, UsersDTO>().ReverseMap();

            CreateMap<Users, UserDTO>().ReverseMap();
            CreateMap<Users, CreateConsumerDTO>().ReverseMap();

            CreateMap<CreateUsersDTO, CreateConsumerDTO>().ReverseMap();
            CreateMap<Users, ConsumerDTO>().ReverseMap();
            CreateMap<Users, ChangePasswordDTO>().ReverseMap();
            CreateMap<Users, SignInConsumerDTO>().ReverseMap();
            CreateMap<Users, VerifyConsumerPasswordDTO>().ReverseMap();

            CreateMap<ConsumerWalletDTO, ConsumerProfile>().ReverseMap();

            CreateMap<tbl_user_on_boarding, OnboardingDTO>().ReverseMap();

            CreateMap<tbl_OTP, CreateOTPDTO>().ReverseMap();
            CreateMap<OTP, SendOTPDTO>().ReverseMap();
            CreateMap<OTP, VerifyOTPDTO>().ReverseMap();

            CreateMap<SecurityAnswer, CreateSecurityAnswerDTO>().ReverseMap();

            CreateMap<Users, ChangeTransactionPinDTO>().ReverseMap();
            CreateMap<Users, ResetTransactionPinDTO>().ReverseMap();
            CreateMap<Users, VerifyTransactionPinDTO>().ReverseMap();

            CreateMap<Users, AgencyDTO>().ReverseMap();
            CreateMap<Users, CreateAgencyDTO>().ReverseMap();
            CreateMap<Users, SignInAgencyDTO>().ReverseMap();
            CreateMap<Users, UpdateAgencyPasswordDTO>().ReverseMap();
            CreateMap<Users, UploadBusinessDocumentDTO>().ReverseMap();
            CreateMap<Users, UploadSelfieDTO>().ReverseMap();

            CreateMap<Auth, CreateAdminDTO>().ReverseMap();
            CreateMap<AdminData, ReadAdminDTO>().ReverseMap();

            CreateMap<SecurityAnswer, SecurityAnswerDTO>().ReverseMap();
            CreateMap<SecurityQuestion, SecurityQuestionDTO>().ReverseMap();

            CreateMap<SecurityQuestion, SecurityQuestionDTO>().ReverseMap();
            CreateMap<TokenResponseDTO, TokenResponse>().ReverseMap();
            CreateMap<KycDTO, SmileRequestDTO>().ReverseMap();
            CreateMap<SmileIdVerification, VerificationResponseDTO>().ReverseMap();
            CreateMap<SmileIdVerification, Actions>().ReverseMap();
        }
    }
}