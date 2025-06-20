using Microsoft.AspNetCore.Identity;

namespace Presentation.Data.Entities;

public class UserEntity : IdentityUser
{
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
  public DateTime Created { get; set; } = DateTime.Now;
}