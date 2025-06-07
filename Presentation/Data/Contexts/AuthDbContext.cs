using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Presentation.Data.Entities;

namespace Presentation.Data.Contexts;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<UserEntity>(options) { }