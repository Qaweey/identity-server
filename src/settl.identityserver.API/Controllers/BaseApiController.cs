using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using settl.identityserver.Application.Contracts.DTO.Users;
using settl.identityserver.Domain.Models;
using settl.identityserver.Domain.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace settl.identityserver.API.Controllers
{
    [Route("identityserver/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class BaseApiController : ControllerBase
    {
        public readonly string _requestID;
        protected IHttpContextAccessor _httpContextAccessor;
        protected readonly Dictionary<CustomApiResponse.Status, string> Responses = CustomApiResponse.Get();
        public UserDTO SettlUser;

        public BaseApiController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _requestID = _httpContextAccessor.HttpContext.Request.Headers["x-RequestID"];
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected List<string> GetModelStateErrors(ModelStateDictionary modelState)
        {
            var errors = new List<string>();
            foreach (var state in modelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            return errors;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult ApiUnauthorized(string message = "Unauthorized")
        {
            var r = new Response<List<string>>
            {
                Code = Responses[CustomApiResponse.Status.API_NOT_AUTHORIZED],
                Message = message,
                Data = null,
                Errors = null
            };

            return Unauthorized(r);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected IActionResult ApiBadModel(ModelStateDictionary modelState = null, List<string> errors = null, string message = "A required parameter is missing from the input. See errors")
        {
            var r = new Response<object>
            {
                Code = Responses[CustomApiResponse.Status.MISSING_REQUIRED_PARAMETER],
                Message = message,
                Data = null,
                Errors = modelState is null ? errors is not null ? errors : null : GetModelStateErrors(modelState)
            };

            return BadRequest(r);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected IActionResult ApiBad(List<string> errors, string message = "")
        {
            var r = new Response<List<string>>
            {
                Code = Responses[CustomApiResponse.Status.MISSING_REQUIRED_PARAMETER],
                Message = message,
                Data = null,
                Errors = errors
            };
            return BadRequest(r);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected IActionResult ApiConflict(string message = "FAILED")
        {
            var responses = CustomApiResponse.Get();
            var r = new Response<string>
            {
                Code = responses[CustomApiResponse.Status.FAILED],
                Message = message,
                Data = null,
                Errors = null
            };
            return Conflict(r);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected IActionResult ApiException(Exception ex)
        {
            var responses = CustomApiResponse.Get();
            var errors = new List<string>();
            do
            {
                errors.Add(ex.Message);
                ex = ex.InnerException;
            } while (ex != null);

            var r = new Response<string>
            {
                Code = responses[CustomApiResponse.Status.INTERNAL_ERROR],
                Message = CustomApiResponse.Status.INTERNAL_ERROR.ToString(),
                Data = null,
                Errors = errors
            };

            return BadRequest(r);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected IActionResult ApiNotFound(string message = "resource not found")
        {
            var r = new Response<string>
            {
                Code = "61",
                Message = message,
                Data = null,
                Errors = null
            };
            return NotFound(r);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected IActionResult ApiOk<T>(T obj, string message = "SUCCESS", string code = "00")
        {
            var r = new Response<T>
            {
                Code = code,
                Message = message,
                Data = obj,
                Errors = null
            };
            return Ok(r);
        }

        public string GetFieldsToUpdate<TSource>(TSource obj, string fields)
        {
            List<string> lstOfFields = new List<string>();
            var fieldsToUpdate = new List<string>();
            lstOfFields = fields.Split(',').ToList();
            List<string> lstOfFieldsToWorkWith = new List<string>(lstOfFields);
            if (!lstOfFieldsToWorkWith.Any())
            {
                return fields;
            }
            else
            {
                foreach (var field in lstOfFieldsToWorkWith)
                {
                    var fieldProp = obj.GetType()
                        .GetProperty(field.Trim(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    var fieldValue = fieldProp.GetValue(obj, null);
                    if (!String.IsNullOrWhiteSpace(fieldValue?.ToString())) fieldsToUpdate.Add(fieldProp.Name);
                }
            }
            return string.Join(",", fieldsToUpdate);
        }
    }
}