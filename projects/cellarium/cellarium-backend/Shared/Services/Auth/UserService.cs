using System.Security.Claims;
using cellarium_backend.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace cellarium_backend.Shared.Services.Auth;

public class UserService() : IUserService
{
    
    public Task<User> GetUser(HttpContext context)
    {
        // Extract user ID directly from claims (JWT auth already completed)
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim != null && !string.IsNullOrEmpty(userIdClaim.Value))
        {
            return Task.FromResult(new User { Id = userIdClaim.Value });
        }
        
        // Return temp user if no valid claim found
        return Task.FromResult(new User { Id = "temp" });
    }
    
}