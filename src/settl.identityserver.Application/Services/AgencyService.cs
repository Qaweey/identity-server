using AutoMapper;
using Microsoft.EntityFrameworkCore;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO.Agency;
using settl.identityserver.Application.Contracts.DTO.Onboarding;
using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class AgencyService : IAgencyService
    {
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Users> _usersRepository;
        private readonly IGenericRepository<tbl_user_on_boarding> _userOnboardingRepository;
        private readonly IGenericRepository<SecurityAnswer> _consumerSecurityAnswerRepository;

        public AgencyService(IMapper mapper, IGenericRepository<Users> usersRepository, IGenericRepository<tbl_user_on_boarding> userOnboardingRepository, IGenericRepository<SecurityAnswer> consumerSecurityAnswerRepository)
        {
            _mapper = mapper;
            _usersRepository = usersRepository;
            _userOnboardingRepository = userOnboardingRepository;
            _consumerSecurityAnswerRepository = consumerSecurityAnswerRepository;
        }

        public async Task<ResponsesDTO> CreateAgency(CreateAgencyDTO createAgencyDTO)
        {
            OnboardingDTO onboardingDTO = new OnboardingDTO()
            {
                Phone = createAgencyDTO.Phone,
                Stage = Constants.CREATE_AGENCY_ACCOUNT
            };

            var onboarding = await OnboardingService.Create(onboardingDTO, _mapper, _userOnboardingRepository);

            bool isValidEmail = RegexHelper.IsValidEmail(createAgencyDTO.Email);

            if (!isValidEmail)
            {
                return Responses.BadRequest(message: "Invalid email address.");
            }

            Users user = _usersRepository.Query().Where(x => x.Email == createAgencyDTO.Email || x.Phone == createAgencyDTO.Phone).FirstOrDefault();

            if (user != null)
            {
                return Responses.BadRequest(message: "Email or Phone number already taken.");
            }

            createAgencyDTO.Password = BCrypt.Net.BCrypt.HashPassword(createAgencyDTO.Password);

            var agency = _mapper.Map<Users>(createAgencyDTO);
            agency.IsFirstTimeLogin = true;

            var _agency = _mapper.Map<AgencyDTO>(createAgencyDTO);

            await _usersRepository.Create(agency);
            await _usersRepository.Save();

            return Responses.Ok(_agency, "You have signed up successfully!");
        }

        public async Task<ResponsesDTO> SignInAgency(SignInAgencyDTO signInAgencyDTO)
        {
            var user = await _usersRepository.Query().Where(x => x.Phone == signInAgencyDTO.Phone).FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(signInAgencyDTO.Password, user.Password))
            {
                return Responses.NotFound("Invalid login details. User not found.");
            }

            if (user.IsFirstTimeLogin)
            {
                OnboardingDTO onboardingDTO = new OnboardingDTO()
                {
                    Phone = signInAgencyDTO.Phone,
                    Stage = Constants.SIGNIN_USER
                };

                var onboarding = await OnboardingService.Create(onboardingDTO, _mapper, _userOnboardingRepository);
            }

            user.LastLoginDate = DateTime.UtcNow;
            user.LoggedOnStatus = Constants.ActiveStatus;
            user.LockOutStatus = Constants.ActiveStatus;

            _usersRepository.Update(user);
            await _usersRepository.Save();

            return Responses.Ok(user);
        }

        public async Task<ResponsesDTO> UpdateAgencyPassword(UpdateAgencyPasswordDTO updateAgencyPasswordDTO)
        {
            updateAgencyPasswordDTO.Password = BCrypt.Net.BCrypt.HashPassword(updateAgencyPasswordDTO.Password);
            var user = _mapper.Map<Users>(updateAgencyPasswordDTO);

            _usersRepository.Update(user);
            await _usersRepository.Save();

            return Responses.Ok(user);
        }

        public async Task<ResponsesDTO> UploadAgencyBusinessDocument(UploadBusinessDocumentDTO uploadBusinessDocumentDTO)
        {
            try
            {
                OnboardingDTO onboardingDTO = new OnboardingDTO()
                {
                    Phone = uploadBusinessDocumentDTO.Phone,
                    Stage = Constants.UPLOAD_BUSINESS_DOCUMENT
                };

                var onboarding = await OnboardingService.Create(onboardingDTO, _mapper, _userOnboardingRepository);

                if (String.IsNullOrEmpty(uploadBusinessDocumentDTO.BusinessName) || String.IsNullOrEmpty(uploadBusinessDocumentDTO.BusinessAddress))
                {
                    return Responses.BadRequest(message: "Business Name and Business Address are required");
                }

                if (String.IsNullOrEmpty(uploadBusinessDocumentDTO.CACDocument.FileName) || String.IsNullOrEmpty(uploadBusinessDocumentDTO.UtilityBill.FileName))
                {
                    return Responses.BadRequest(message: "You must upload your CAC Document or Utility Bill.");
                }

                var path1 = Path.Combine(Directory.GetCurrentDirectory(), "BusinessDocuments", uploadBusinessDocumentDTO.CACDocument.FileName);
                var stream1 = new FileStream(path1, FileMode.Create);
                await uploadBusinessDocumentDTO.CACDocument.CopyToAsync(stream1);

                var path2 = Path.Combine(Directory.GetCurrentDirectory(), "BusinessDocuments", uploadBusinessDocumentDTO.UtilityBill.FileName);
                var stream2 = new FileStream(path2, FileMode.Create);
                await uploadBusinessDocumentDTO.UtilityBill.CopyToAsync(stream2);

                var agencyDocument = _mapper.Map<Users>(uploadBusinessDocumentDTO);

                _usersRepository.Update(agencyDocument);
                await _usersRepository.Save();

                return Responses.Ok(uploadBusinessDocumentDTO, "Documents uploaded successfully.");
            }
            catch
            {
                return Responses.BadRequest(message: "Oops! Problem uploading documents ");
            }
        }

        public async Task<ResponsesDTO> UploadAgencySelfie(UploadSelfieDTO uploadSelfieDTO)
        {
            OnboardingDTO onboardingDTO = new OnboardingDTO()
            {
                Phone = uploadSelfieDTO.Phone,
                Stage = Constants.UPLOAD_SELFIE
            };

            var onboarding = await OnboardingService.Create(onboardingDTO, _mapper, _userOnboardingRepository);

            if (String.IsNullOrEmpty(uploadSelfieDTO.Selfie))
            {
                return Responses.BadRequest(message: "Please upload your selfie.");
            }

            var agencySelfie = _mapper.Map<Users>(uploadSelfieDTO);

            _usersRepository.Update(agencySelfie);
            await _usersRepository.Save();

            return Responses.Ok(agencySelfie, "Selfie uploaded successfully.");
        }

        public async Task<ResponsesDTO> CreateAgencySecurityAnswer(List<CreateSecurityAnswerDTO> createSecurityAnswerDTO)
        {
            foreach (var ans in createSecurityAnswerDTO)
            {
                if (String.IsNullOrEmpty(ans.Answer))
                {
                    return Responses.BadRequest(message: "Security answer cannot be empty.");
                }
            }

            List<SecurityAnswer> csa = _mapper.Map<List<SecurityAnswer>>(createSecurityAnswerDTO);

            await _consumerSecurityAnswerRepository.AddRange(csa);
            await _consumerSecurityAnswerRepository.Save();

            return Responses.Ok("Security Answer saved successfully.");
        }

        public async Task<ResponsesDTO> VerifyAgencyNIN(VerifyAgencyNINDTO verifyAgencyNINDTO)
        {
            NINDTO jsonData = new NINDTO()
            {
                sec_key = "j7EZZxIMw5TBKkPJcpIaJ417czlUhYr/JtY3Wo1dIMLmuEzSgQfOhtlA1L+ThbWT/Ukh+nnxUPu2I3PHPiKtflHcx1Fpjq5HHDFNbyWOLk9Q4WbJt22S1xybFfUHZ6YOcODeKc5NLLC6+zriAtDG2Veqkn33E+XOwmri2uP+v08=|aed18f2af4b850c2c33c8b93dc30faa7596ea1b6fdad967ef43fc64aebb47751",

                timestamp = "1622666871",

                partner_params = new PartnerParam()
                {
                    job_id = DateTime.UtcNow.Millisecond.ToString(),
                    user_id = "Ig6mdzsrXSKInurQLUTTkVx3",
                    job_type = 5
                },

                country = "NG",
                id_type = "NIN",
                id_number = verifyAgencyNINDTO.NIN,
                partner_id = "1275"
            };

            HttpClient client = new HttpClient();

            var response = await client.PostAsJsonAsync("https://testapi.smileidentity.com/v1/id_verification", jsonData);

            if (!response.IsSuccessStatusCode)
            {
                return Responses.BadRequest(message: "Oops! Problem sending verification NIN.");
            }

            return Responses.Ok("NIN verification successful.");
        }
    }
}