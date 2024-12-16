using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO.KYC;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Dapper;
using settl.identityserver.Domain.Entities;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static settl.identityserver.Domain.Shared.Enums.KYC;

namespace settl.identityserver.API.Controllers
{
    [Route("identityserver/verify")]
    [ApiController]
    public class VerificationController : BaseApiController
    {
        private readonly IVerificationService service;
        private readonly IGenericRepository<SmileIdVerification> genericService;
        private readonly IMapper _mapper;

        public VerificationController(IVerificationService verificationService, IMapper mapper, IHttpContextAccessor httpContextAccessor, IGenericRepository<SmileIdVerification> genericRepository
            , IGenericRepository<SelfieVerification> selfieRepository
            ) : base(httpContextAccessor)
        {
            _mapper = mapper;
            service = verificationService;
            genericService = genericRepository;
        }

        /// <summary>
        /// Verify the Identity Information for an individual using their personal information and the ID number from one of our supported ID Types.
        /// </summary>
        /// <param name="kyc"> 1. NIN - 00000000000, 2. NIN_SLIP - 0000000000, 3. DRIVERS_LICENSE - ABC000000000, 4. VOTER_ID 0000000000000000000 5. CAC RC0000000 6. TIN 00000000-0000 7. BVN 00000000000 <br /> <br />
        ///  DOB - 2000-01-20(YYYY-MM-DD), First Name - Doe, Last Name - Joe, Gender - M, Phone Number - 1234567890
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> VerifyUser([FromBody] KycDTO kyc)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return ApiBad(modelErrors, modelErrors[0]);
            }

            var error = ValidateIdNumber(kyc.Id_type, kyc.Id_number);

            if (error is not null) return ApiBad(null, error);

            if (!string.IsNullOrEmpty(kyc?.Dob) && !DateTime.TryParse(kyc.Dob, out _)) return ApiBad(null, "Invalid date of birth");

            var (success, response) = await service.VerifyKYC(kyc);

            if (!success) return ApiBad(null, response?.Error ?? response.ResultText);

            var model = _mapper.Map<SmileIdVerification>(response);
            _mapper.Map(response.Actions, model);

            //model.User_Id = response.PartnerParams.UserId;
            //model.Job_Id = response.PartnerParams.JobId;
            //model.Job_Type = response.PartnerParams.JobType;
            //model.SecKey = response.Sec_Key;
            //await genericService.Create(model);
            //await genericService.Save();

            return success ? ApiOk(new { model.DOB, model.Gender, model.Names, model.Phone_Number, model.ID_Verification, response.ResultText, model.Verify_ID_Number }) : ApiBad(null, model?.ResultText);
        }

        /// <summary>
        /// Verify the Identity Information for an individual using their personal information and the ID number from one of our supported ID Types.
        /// </summary>
        /// <param name="selfie"> 1. Smart Selfie<br /> <br />
        ///  file_name - xxxxxx.zip, smile client Id - partnerId, signature - , timestanp - 1234567890
        /// </param>
        /// <returns></returns>
        [HttpPost("UserSelfie")]
        public async Task<IActionResult> VerifyUserSelfie([FromBody] Welcome2 uploadParam)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return ApiBad(modelErrors, modelErrors[0]);
            }

            var (success, response) = await service.VerifySelfie(uploadParam);

            if (!success) return ApiConflict(response.Error);

            //var result=await service.UpdateSelfie(response);
            if (success)
            {
                return ApiOk(new { response.ConfidenceValue, response.ResultText });
            }
            return success ? ApiOk(new { response.ResultText, response.ResultCode }) : ApiConflict();
        }

        private static string ValidateIdNumber(IdType idType, string number)
        {
            return idType switch
            {
                IdType.NIN => Regex.Match(number, "^[0-9]{11}$").Success ? null : "Invalid NIN",
                IdType.NIN_SLIP => Regex.Match(number, "^[0-9]{11}$").Success ? null : "Invalid NIN Slip Number",
                IdType.DRIVERS_LICENSE => Regex.Match(number, "^(?=.*[0-9])(?=.*[A-Z])[A-Z0-9]{3}([ -]{1})?[A-Z0-9]{6,12}$", RegexOptions.IgnoreCase).Success ? null : "Invalid Drivers License Number",
                IdType.VOTER_ID => Regex.Match(number, "^([A-Z0-9]{19}|[A-Z0-9]{9})$", RegexOptions.IgnoreCase).Success ? null : "Invalid Voter ID",
                IdType.CAC => Regex.Match(number, "^(RC)?[0-9]{5,8}$", RegexOptions.IgnoreCase).Success ? null : "Invalid CAC number",
                IdType.TIN => Regex.Match(number, "^[0-9]{8,}-[0-9]{4,}$").Success ? null : "Invalid TIN",
                IdType.BVN => Regex.Match(number, "^[0-9]{11}$").Success ? null : "Invalid BVN",
                _ => null,
            };
        }

        /// <summary>
        /// Smile ID will send the results of a job
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost("callback")]
        public async Task<IActionResult> CreateSmileResponse([FromBody] VerificationResponseDTO payload)
        {
            var model = _mapper.Map<SmileIdVerification>(payload);
            _mapper.Map(payload.Actions, model);

            model.User_Id = payload.PartnerParams.UserId;
            model.Job_Id = payload.PartnerParams.JobId;
            model.Job_Type = payload.PartnerParams.JobType;
            model.SecKey = payload.Sec_Key;
            await genericService.Create(model);
            var success = await genericService.Save();

            return success ? ApiOk<object>(null) : ApiBad(null, message: "Failed to process events");
        }
    }
}