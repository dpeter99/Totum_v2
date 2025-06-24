using cellarium_backend.Shared.Models;

namespace cellarium_backend.Shared.Services.Auth;

public interface IUserService
{
    public Task<User> GetUser(HttpContext context);
}