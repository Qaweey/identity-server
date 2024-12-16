using System;
using System.Text;
using BCryptNet = BCrypt.Net.BCrypt;

namespace settl.identityserver.Domain.Shared.Helpers.Cryptography
{
    public static class Hashing
    {
        public static string Base64StringEncode(string originalString = null)
        {
            if (string.IsNullOrEmpty(originalString)) return null;

            var bytes = Encoding.Default.GetBytes(originalString);

            var encodedString = Convert.ToBase64String(bytes);

            return encodedString;
        }

        public static string Base64StringDecode(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return null;

            var bytes = Convert.FromBase64String(base64String);

            var decodedString = Encoding.UTF8.GetString(bytes);

            return decodedString;
        }

        public static bool IsBase64String(string base64)
        {
            var buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }

        private static string GetRandomSalt()
        {
            return BCryptNet.GenerateSalt(12);
        }

        public static string HashPassword(string password)
        {
            return BCryptNet.HashPassword(password, GetRandomSalt());
        }

        public static bool ValidatePassword(string password, string correctHash)
        {
            return BCryptNet.Verify(password, correctHash);
        }

    }
}