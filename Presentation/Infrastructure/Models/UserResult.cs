namespace Presentation.Infrastructure.Models;

public class UserResult<T> : BaseResult
{
  public T? Result { get; set; }
}

public class UserResult : BaseResult { }