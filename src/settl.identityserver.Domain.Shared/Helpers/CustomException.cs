using System;

namespace settl.identityserver.Domain.Shared.Helpers
{
    public class CustomException : ApplicationException
    {
        public CustomException(string message = "An unexpected error occured. Please try again later") : base(message)
        {
        }
    }
}