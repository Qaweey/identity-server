using System.Collections.Generic;

namespace settl.identityserver.Domain.Shared.Helpers
{
    public class CustomApiResponse
    {
        public enum Status
        {
            SUCCESS = 0,
            INVALID_BASIC_AUTHORIZATION = 1,
            INVALID_CREDENTIALS = 2,
            INVALID_API_OPERATION = 3,
            INVALID_PARAMETERS = 4,
            INTERNAL_ERROR = 6,
            API_NOT_AUTHORIZED = 7,
            MISSING_REQUIRED_PARAMETER = 31,
            FAILED = 70
        }

        public static Dictionary<Status, string> Get()
        {
            var responseMessages = new Dictionary<Status, string>
            {
                {Status.SUCCESS, "00" },
                {Status.INVALID_BASIC_AUTHORIZATION, "01" },
                {Status.INVALID_CREDENTIALS, "02" },
                {Status.INVALID_API_OPERATION, "03" },
                {Status.INVALID_PARAMETERS, "04" },
                {Status.INTERNAL_ERROR, "06" },
                {Status.API_NOT_AUTHORIZED, "07" },
                 {Status.MISSING_REQUIRED_PARAMETER, "31" },
                {Status.FAILED, "70" },
            };

            return responseMessages;
        }
    }
}