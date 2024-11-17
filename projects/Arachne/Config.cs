using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Arachne;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope(name: "api1", displayName: "API 1"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // m2m client credentials flow client
            new Client
            {
                ClientId = "web",
                ClientName = "Sample Web Client",
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "http://localhost:5002/signin-oidc" },
                PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },
                
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                }
            },
            new Client
            {
                ClientId = "react-client",
                ClientName = "React Client",
                ClientSecrets = { new Secret("901564A5-E7FE-42CB-B10D-61EF6A8F3654".Sha256()) }, 
                
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "http://localhost:6001/oauth/callback" },
                PostLogoutRedirectUris = { "http://localhost:6001/" },
                
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "api1"
                }
            }
        };
}