using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.Users
{
    public class UpdateFcmTokenDTO
    {
        public string FcmToken { get; set; }

        public string Phone { get; set; }
    }

    public class PhoneOSAndFcmTokenModel
    {
        public string FcmToken { get; set; }
        public string PhoneOS { get; set; }
    }

    public class PhoneNameAndTokenModel
    {
        public string FcmToken { get; set; }
        public string PhoneModelNo { get; set; }
    }
}