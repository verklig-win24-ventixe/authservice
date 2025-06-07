namespace Presentation.Domain.Models;

public class User
{
  public string Id { get; set; } = null!;
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
  public string Email { get; set; } = null!;
}