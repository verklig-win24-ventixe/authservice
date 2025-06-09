using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Presentation.Data.Entities;
using Presentation.Interfaces;

namespace Presentation.Services;

public class TokenGenerationService(RsaSecurityKey rsaSecurityKey, string issuer, string audience) : ITokenGenerationService
{
  private readonly RsaSecurityKey _rsaSecurityKey = rsaSecurityKey;
  private readonly string _issuer = issuer;
  private readonly string _audience = audience;

  public string GenerateJwtToken(UserEntity user, IList<string> roles)
  {
    var claims = new List<Claim>
    {
      new(ClaimTypes.Name, user.UserName!),
      new(ClaimTypes.NameIdentifier, user.Id),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var creds = new SigningCredentials(_rsaSecurityKey, SecurityAlgorithms.RsaSha256);

    var token = new JwtSecurityToken(
      issuer: _issuer,
      audience: _audience,
      claims: claims,
      expires: DateTime.UtcNow.AddDays(3),
      signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}