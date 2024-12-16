using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using settl.identityserver.Application.Contracts.DTO.Agency;
using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Application.Contracts.IServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace settl.identityserver.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AgencyController : ControllerBase
    {
        private readonly IAgencyService _agencyService;

        public AgencyController(IAgencyService agencyService)
        {
            _agencyService = agencyService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] CreateAgencyDTO createAgencyDTO)
        {
            var agency = await _agencyService.CreateAgency(createAgencyDTO);

            return Ok(agency);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> LogIn([FromBody] SignInAgencyDTO signInAgencyDTO)
        {
            var agency = await _agencyService.SignInAgency(signInAgencyDTO);

            var client = new HttpClient();

            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = Environment.GetEnvironmentVariable("IDENTITYSERVER_TOKEN_ENDPOINT"),

                ClientId = Environment.GetEnvironmentVariable("ClientId"),

                ClientSecret = Environment.GetEnvironmentVariable("ClientSecret"),

                Scope = "sms.read sms.write"
            });

            if (response.IsError) throw new Exception(response.Error);

            return Ok(new
            {
                accessToken = response.AccessToken,
                expiresIn = response.ExpiresIn,
                tokenType = response.TokenType,
                data = agency.Data
            });
        }

        [HttpPatch("upload/business-document")]
        public async Task<IActionResult> UploadBusinessDocument(UploadBusinessDocumentDTO uploadBusinessDocumentDTO)
        {
            var agencyDocument = await _agencyService.UploadAgencyBusinessDocument(uploadBusinessDocumentDTO);

            return Ok(agencyDocument);
        }

        [HttpPatch("upload/selfie")]
        public async Task<IActionResult> UploadSelfie(UploadSelfieDTO uploadSelfieDTO)
        {
            var agencySelfie = await _agencyService.UploadAgencySelfie(uploadSelfieDTO);

            return Ok(agencySelfie);
        }

        [HttpPost("create/security-answer")]
        public async Task<IActionResult> CreateSecurityAnswer([FromBody] List<CreateSecurityAnswerDTO> createSecurityAnswerDTO)
        {
            var csa = await _agencyService.CreateAgencySecurityAnswer(createSecurityAnswerDTO);

            return Ok(csa);
        }

        [HttpPost("verify/nin")]
        public async Task<IActionResult> VerifyNIN([FromBody] VerifyAgencyNINDTO verifyAgencyNINDTO)
        {
            var nin = await _agencyService.VerifyAgencyNIN(verifyAgencyNINDTO);

            return Ok(nin);
        }
    }
}