using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using settl.identityserver.Application.Contracts.DTO.Consumer;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.RepositoryInterfaces;
using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace settl.identityserver.API.Controllers
{
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserController(IMapper mapper, IGenericRepository<Users> usersRepository, IUserService userService, IHttpContextAccessor httpContextAccessor) :
            base(httpContextAccessor)
        {
            _userService = userService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Create([FromBody] CreateUsersDTO request)
        {
            var responses = CustomApiResponse.Get();
            IActionResult dataResult = null;

            try
            {
                if (!ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                    dataResult = ApiBad(modelErrors, responses[CustomApiResponse.Status.INVALID_PARAMETERS], CustomApiResponse.Status.INVALID_PARAMETERS.ToString());
                    return dataResult;
                }

                Log.Information(SerializeUtility.SerializeJSON(request));

                try
                {
                    var signup = await _userService.SignUpUser(request);

                    dataResult = ApiOk(signup);
                }
                catch (CustomException ex)
                {
                    Log.Error(ex.Message);
                    dataResult = ApiConflict(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                dataResult = ApiException(ex);
            }

            return dataResult;
        }


    }
}
