using System.Collections.Generic;

namespace settl.identityserver.Domain.Shared
{
    public static class Responses
    {
        // todo create generic responses for Ok, Created, Unauthorize, BadRequest
        public static ResponsesDTO Ok<T>(T obj, string message = "success")
        {
            return new ResponsesDTO()
            {
                Code = "00",
                Message = message,
                Data = obj,
                Errors = null
            };
        }

        public static ResponsesDTO Created(object obj = null, string message = "success")
        {
            return new ResponsesDTO()
            {
                Code = "00",
                Message = message,
                Data = obj
            };
        }

        public static ResponsesDTO BadRequest(List<string> errors = null, string message = "bad request")
        {
            return new ResponsesDTO()
            {
                Code = "70",
                Message = message,
                Data = "",
                Errors = errors
            };
        }

        public static ResponsesDTO NotFound(string message = "data not found")
        {
            return new ResponsesDTO()
            {
                Code = "61",
                Message = message,
                Data = "",
            };
        }

        public static ResponsesDTO Deleted(string message = "deleted successfully")
        {
            return new ResponsesDTO()
            {
                Code = "00",
                Message = message,
                Data = "",
            };
        }

        public static ResponsesDTO Conflict(string message = "resource already exist")
        {
            return new ResponsesDTO()
            {
                Code = "70",
                Data = "",
                Message = message,
            };
        }
    }

    public static class ResponseStatus
    {
        public const string Success = "success", Fail = "fail";
    }

    public enum ResponseCode
    {
        NotFound
    }
}