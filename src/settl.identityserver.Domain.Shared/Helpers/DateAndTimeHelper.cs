using System;

namespace settl.identityserver.Domain.Shared.Helpers
{
    public class DateAndTimeHelper
    {

        public static DateTime GetCurrentServerTime()
        {
            //DateTimeOffset currentLocalTime = DateTimeOffset.Now;

            //DateTimeOffset currentUtcTime = DateTimeOffset.UtcNow;

            //var timeDifference = currentLocalTime.Offset.ToString();

            //if(timeDifference.StartsWith("+"))
            //    currentLocalTime = currentLocalTime

            return DateTime.UtcNow.AddHours(1);
        }

    }
}
