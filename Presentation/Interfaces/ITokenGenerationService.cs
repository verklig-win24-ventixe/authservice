using Presentation.Data.Entities;

namespace Presentation.Interfaces;

public interface ITokenGenerationService
{
  string GenerateJwtToken(UserEntity user, IList<string> roles);
}