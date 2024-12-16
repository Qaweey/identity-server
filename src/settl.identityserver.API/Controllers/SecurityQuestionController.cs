using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Shared.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace settl.identityserver.API.Controllers
{
    [Route("identityserver")]
    public class SecurityQuestionController : BaseApiController
    {
        private readonly ISecurityQuestionService _securityAnswerService;

        public SecurityQuestionController(ISecurityQuestionService securityQuestionService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _securityAnswerService = securityQuestionService;
        }

        [HttpGet("security-questions")]
        public IActionResult GetSecurityQuestion()
        {
            var responses = CustomApiResponse.Get();
            IActionResult dataResult = null;

            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                dataResult = ApiBad(modelErrors, modelErrors[0]);
                return dataResult;
            }

            try
            {
                var result = _securityAnswerService.GetSecurityQuestion();

                dataResult = ApiOk(result);
            }
            catch (CustomException ex)
            {
                Log.Error(ex.Message);
                dataResult = ApiConflict(ex.Message);
            }

            return dataResult;
        }

        [HttpGet("security-question/selected")]
        public async Task<IActionResult> GetSelectedSecurityQuestion([FromQuery, Required, RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")] string phone)
        {
            var gsq = await _securityAnswerService.GetSelectedSecurityQuestion(phone);

            if (gsq.Code != "00")
            {
                return Conflict(gsq);
            }

            return Ok(gsq);
        }
    }
}