using System.Security.Claims;
using cellarium_backend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace cellarium_backend.Services.Auth;

public class UserService() : IUserService
{
    
    public async Task<User> GetUser(HttpContext context)
    {
        
        var res = await context.AuthenticateAsync();
        
        context.User.Identity.IsAuthenticated = true;

        if (res.Succeeded && res.Principal != null)
        {
            var name = res.Principal.FindFirst(ClaimTypes.NameIdentifier);
        
            Console.WriteLine("######: " + name.Value);
        }
        
        return null;
    }
    
}