using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;

namespace settl.identityserver.API
{
    public class Config
    {
        public static IEnumerable<ApiScope> GetScopes()
        {
            return new List<ApiScope>
                {
                   new ApiScope("sms.read", "Read access SMS service"),
                   new ApiScope("sms.write", "Write access SMS service"),
                   new ApiScope("consumer.read", "Read access Consumer service"),
                   new ApiScope("consumer.write", "Write access Consumer service"),
                   new ApiScope("admin.read", "Read access Admin service"),
                   new ApiScope("admin.write", "Write access Admin service"),
                   new ApiScope("agency.read", "Read access Agency service"),
                   new ApiScope("agency.write", "Write access Agency service"),
                   new ApiScope("email.read", "Read access Email service"),
                   new ApiScope("email.write", "Write access Email service"),
                   new ApiScope("offline_access")
                };
        }

        public static IEnumerable<IdentityResource> GetIdentity()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email()
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                        ClientName = "ropwd-flow",
                        ClientId = Environment.GetEnvironmentVariable("ClientId"),
                        ClientSecrets = { new Secret(Environment.GetEnvironmentVariable("ClientSecret").Sha256()) },
                        AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                        AllowedScopes = {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "offline_access",
                            "sms.read",
                            "sms.write",
                            "consumer.read",
                            "consumer.write",
                            "admin.read",
                            "admin.write",
                            "agency.read",
                            "agency.write",
                            "email.read",
                            "email.write"
                        },
                        AccessTokenLifetime = 86400,
                        AllowOfflineAccess = true,
                        RedirectUris = { "https://settl.me"}
                }
            };
        }
    }
}