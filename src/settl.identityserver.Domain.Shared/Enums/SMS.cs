namespace settl.identityserver.Domain.Shared.Enums
{
    public enum SMSStatus
    {
        Sent,
        Queued
    }

    public enum OtpType
    {
        SIGN_IN = 1,
        PHONE_NUMBER_VALIDATION,
        FORGOT_PASSWORD,
        CHANGE_PASSWORD,
        REGISTER_NEWDEVICE
         
    }
}