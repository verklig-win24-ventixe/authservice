namespace Presentation.Infrastructure.Models;

public class AuthResult<T> : BaseResult
{ 
  public T? Result { get; set; }
}

public class AuthResult : BaseResult { }