using cellarium_backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace cellarium_backend;

public class CellariumDbContext: DbContext
{
    public CellariumDbContext(DbContextOptions options) : base(options)
    {
    }
}