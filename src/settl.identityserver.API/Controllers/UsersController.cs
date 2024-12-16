using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Shared.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace settl.identityserver.API.Controllers
{
    [ApiController]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.Get();

            return ApiOk(users);
        }

        /// <summary>
        /// Get a user by their phone number
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet("{phone}")]
        public async Task<IActionResult> Get([Required, RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number")] string phone)
        {
            phone = phone.Trim();
            var user = await _userService.Get(phone);

            return user is null ? ApiNotFound("User not found") : ApiOk(user);
        }

        [HttpGet("fcmtokenandphoneos")]
        public async Task<IActionResult> GetFcmTokenAndPhoneOS([Required] string phone)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return ApiBadModel(ModelState, modelErrors, modelErrors[0]);
            }

            phone = phone.Trim();
            var (success, result) = await _userService.GetFcmTokenAndPhoneOS(phone);

            if (!success) return ApiBad(null, message: "error retrieving Fcm Token or phone OS");

            return ApiOk(result);
        }

        [HttpPut("fcmtoken")]
        public async Task<IActionResult> UpdateFcmToken(UpdateFcmTokenDTO model)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                return ApiBadModel(ModelState, modelErrors, modelErrors[0]);
            }

            if (!string.IsNullOrEmpty(model.Phone)) model.Phone = model.Phone.Trim();

            Log.Information(JsonHelper.SerializeObject(model));

            var (success, message) = await _userService.UpdateFcmToken(model);

            if (!success)
            {
                Log.Error(message);
            }

            return ApiOk(message);
        }

        /// <summary>
        /// Get Users with number of days
        /// </summary>
        /// <param name="query">30 for the last 30 days, pass 0 to get data for today
        ///  <p>2021-08-09</p>
        /// </param>
        /// <returns></returns>
        [HttpGet("range")]
        public async Task<IActionResult> GetUsersWithDate([FromQuery, Required] UserDateFilter query)
        {
            if (query.Days < 0) return ApiBad(null, message: "Invalid number of days");

            var enteredDates = query.StartDate.Length > 0 || query.EndDate.Length > 0;

            if (query.Days >= 0 && enteredDates) return ApiBad(null, message: "Filter by number of days or customized dates, not both.");

            if (!enteredDates && query.Days is null) return ApiBad(null, message: "Invalid query parameters");

            if (enteredDates)
            {
                var error = ValidateDateRange(query.StartDate, query.EndDate);
                if (error is not null) return ApiBad(null, message: error);
            }

            var result = await _userService.GetUsersFilter(query.Days, query.StartDate, query.EndDate);

            return ApiOk(result);
        }

        /// <summary>
        /// Get breakdown of app downloads of consumers and agents
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("appdownloads")]
        public async Task<IActionResult> GetAppDownloads([FromQuery, Required] UserDateFilter query)
        {
            if (query?.Days < 0) return ApiBad(null, message: "Invalid number of days");

            var enteredDates = query.StartDate.Length > 0 || query.EndDate.Length > 0;

            if (query?.Days >= 0 && enteredDates) return ApiBad(null, message: "Filter by number of days or customized dates, not both.");

            if (!enteredDates && query?.Days is null) return ApiBad(null, message: "Invalid query parameters");

            if (query.Days is null) query.Days = 0;

            if (enteredDates)
            {
                var error = ValidateDateRange(query.StartDate, query.EndDate);
                if (error is not null) return ApiBad(null, message: error);
            }
            var result = await _userService.AppDownloads((int)query?.Days, query.StartDate, query.EndDate);

            return ApiOk(result);
        }

        private static string ValidateDateRange(string start, string end)
        {
            try
            {
                bool isDate = DateTime.TryParse(start, out var startDate);

                if (!isDate) return "Invalid StartDate";

                isDate = DateTime.TryParse(end, out var endDate);

                if (!isDate) return "Invalid EndDate";

                if (startDate > endDate) return "Start Date cannot be greater than End Date";

                if (endDate < startDate) return "End Date cannot be less than Start Date";

                if (endDate > DateTime.Now) return "End date cannot be ahead of today's date";

                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}