namespace Presentation.Infrastructure.Models;

public abstract class BaseResult
{
  public bool Succeeded { get; set; }
  public int StatusCode { get; set; }
  public string? Error { get; set; }
}