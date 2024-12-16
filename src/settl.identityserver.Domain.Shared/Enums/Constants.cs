using System;

namespace settl.identityserver.Domain.Shared.Enums
{
    public class Constants
    {
        public static readonly string DefaultTransactionPIN = "0000";

        public static readonly string TYPE_PHONE_NUMBER = "TYPE_PHONE_NUMBER";

        public static readonly string VERIFY_PHONE_OTP = "VERIFY_PHONE_OTP";
        public static string JWT_SECRET => Environment.GetEnvironmentVariable("JWT_SECRET");

        public static readonly string API_URL = Environment.GetEnvironmentVariable("API_URL");
        public static readonly string ENV_REDIS_CACHE_DB_CONN = Environment.GetEnvironmentVariable("REDIS_CACHE_DB_CONN");
        public static readonly string HANGFIRE_DB = Environment.GetEnvironmentVariable("HANGFIREDB_CONN");
        public static string ASPNETCORE_ENVIRONMENT => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public static readonly string CONSUMER_URL = $"{API_URL}/consumer";

        public static string DB_CONNECTION => Environment.GetEnvironmentVariable("DB_CONNECTION2");

        public static readonly string EMAILSERVICES_URL = $"{API_URL}/emailservices";

        public static readonly string SMSSERVICE_URL = $"{API_URL}/smsservice";

        public static readonly string BANKING_URL = $"{API_URL}/banking";

        public static readonly string BACKOFFICE_URL = $"{API_URL}/backoffice";

        public static readonly string IDENTITYSERVER_URL = $"{API_URL}/identityserver";

        public static readonly string PUSHNOTIFICATIONSERVICE_URL = $"{API_URL}/pushnotification";

        public static readonly string SMILEIDENTITY_URL = Environment.GetEnvironmentVariable("SMILEIDENTITY_URL");
        public static readonly string SMILEIDENTITY_SELFIE = Environment.GetEnvironmentVariable("SMILEIDENTITY_SELFIE");
        public static readonly string SMILEIDENTITY_CALLBACK = Environment.GetEnvironmentVariable("SMILEIDENTITY_CALLBACK");
        public static readonly string SMILE_APIKEY = Environment.GetEnvironmentVariable("SMILE_APIKEY");
        public static readonly string SETTL_API_TOKEN = Environment.GetEnvironmentVariable("SETTL_API_TOKEN");
        public static readonly string SETTL_STORE_PHONE = Environment.GetEnvironmentVariable("SETTL_STORE_PHONE");

        public static readonly string EMAIL_TEMPLATE_FOR_OTP_VERIFICATION = "002";
        public static readonly string EMAIL_TEMPLATE_FOR_OTP_VERIFYNEWDEVICE = "007";
        public static readonly string EMAIL_TEMPLATE_FOR_NEWDEVICE_REGISTRATION = "013";

        public static readonly string SMS_TEMPLATE_FOR_RESET_PASSWORD = "0017";

        public static readonly string SMS_TEMPLATE_FOR_PHONE_VALIDATION = "0001";

        public static readonly string CREATE_CONSUMER_ACCOUNT = "CREATE_CONSUMER_ACCOUNT";

        public static readonly string CREATE_AGENCY_ACCOUNT = "CREATE_AGENCY_ACCOUNT";

        public static readonly string SIGNIN_USER = "SIGNIN_USER";

        public static readonly string UPLOAD_BUSINESS_DOCUMENT = "UPLOAD_BUSINESS_DOCUMENT";

        public static readonly string UPLOAD_SELFIE = "UPLOAD_SELFIE";

        public static readonly string CREATE_CONSUMER_SECURITY_QUESTION = "CREATE_CONSUMER_SECURITY_QUESTION";

        public static readonly string CREATE_CONSUMER_SECURITY_ANSWER = "CREATE_CONSUMER_SECURITY_ANSWER";

        public static readonly string ActiveStatus = "active";

        public static readonly string InActiveStatus = "inactive";

        public static readonly string BLACKLISTED = "BLACKLISTED";

        public static readonly string UNBLACKLISTED = "WHITELISTED";
    }
}