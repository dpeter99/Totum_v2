using cellarium_backend.Models;

namespace cellarium_backend.Services.Auth;

public interface IUserService
{
    public Task<User> GetUser(HttpContext context);
}