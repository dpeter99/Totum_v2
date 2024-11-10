using System.Security.Claims;
using cellarium_backend.Models;
using Microsoft.AspNetCore.Authentication;

namespace cellarium_backend.Services.Auth;

public class UserService(HttpContextAccessor httpContextAccessor)
{
    
    
    
    public async Task<User> GetUser()
    {
        var HttpContext = httpContextAccessor.HttpContext;

        var res = await HttpContext.AuthenticateAsync();

        var name = res.Principal.FindFirst(ClaimTypes.NameIdentifier);
        
        Console.WriteLine("######: " + name.Value);
        
        return null;
    }
    
}