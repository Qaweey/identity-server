using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace settl.identityserver.Domain.Shared.Helpers
{
    public static class StringUtility
    {
        public static string CleanPhone(string phone)
        {
            if (phone != null && phone[0].ToString() == "+")
                return phone.Replace("+", "").Trim();
            return phone;
        }

        public static bool IsValidDialCode(string mobileNumber)
        {
            var validPrefixes = new string[] { "234" };
            if (!validPrefixes.Contains(mobileNumber.Substring(0, 3))) return false;
            return true;
        }

        public static string AppendCodeSign(string phone)
        {
            if (phone != null && phone[0].ToString() != "+")
                return "+" + phone;
            return phone;
        }

        public static string FormatPhoneNumber(string phone)
        {
            if (phone.StartsWith("234") && phone.Length == 13)
            {
                AppendCodeSign(phone);
                return phone;
            }

            if (phone.StartsWith("0") && phone.Length == 11)
            {
                phone.Remove(0, 1);
                return "+234" + phone;
            }

            throw new Exception("Invalid phone format");
        }

        public static string ValidMobile(string mobileNumber)
        {
            var err = new List<string>();
            mobileNumber = AppendCodeSign(mobileNumber);
            mobileNumber = CleanPhone(mobileNumber);
            if (!IsValidDialCode(mobileNumber))
                err.Add("Mobile number has an invalid prefix. please add the dialcode as prefix to the mobile number");
            if (!IsMobile(mobileNumber))
                err.Add("Invalid mobile length or Invalid mobile number format; e.g +2348169325634");
            if (err?.Count > 0)
                return string.Join(",", err.ToArray());
            return string.Empty;
        }

        public static bool IsMobile(string mobile, string countryCode = "ng")
        {
            var isOk = true;
            mobile = CleanPhone(mobile);
            switch (countryCode.ToLower())
            {
                case "ng":
                    if (mobile.Length != 13)
                    {
                        isOk = false;
                    }
                    if (!mobile.StartsWith("234"))
                    {
                        isOk = false;
                    }
                    break;

                default:
                    isOk = false;
                    break;
            }
            return isOk;
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public static (string, bool) HasJWTToken(this HttpRequest Request)
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return ("Authorization header required", true);
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (string.IsNullOrEmpty(authHeader.Parameter))
                return ("Bearer token is required", true);

            return (authHeader.Parameter, false);
        }

        /// <summary>
        /// Capitalize a string.
        /// </summary>
        /// <param name="word"></param>
        /// <returns>The input string with the first character in uppercase.</returns>
        public static string Capitalize(string word)
        {
            if (word is null || word.Length == 0) return word;

            if (word.Length == 1) return char.ToUpper(word[0]).ToString();

            return char.ToUpper(word[0]) + word[1..];
        }
    }
}