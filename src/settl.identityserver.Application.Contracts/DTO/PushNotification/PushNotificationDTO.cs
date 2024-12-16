using System;
using static settl.identityserver.Domain.Shared.Enums.PUSHNOTIFICATION_TYPE;

namespace settl.identityserver.Application.Contracts.DTO.PushNotification
{
    public class PushNotificationResponseDTO
    {
        public string Message { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
        public string Data { get; set; }
    }

    public class PushNotificationRequestDTO
    {
        public string Phone { get; set; }
        public PushNotificationType Type { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string MicroserviceName { get; set; } = Environment.GetEnvironmentVariable("IDENTITYSERVER_URL");
    }
}