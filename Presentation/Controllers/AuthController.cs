using Presentation.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Presentation.Domain.Dtos;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, IConfiguration config) : ControllerBase
{
  private readonly UserManager<UserEntity> _userManager = userManager;
  private readonly SignInManager<UserEntity> _signInManager = signInManager;
  private readonly IConfiguration _config = config;

  [HttpPost("register")]
  public async Task<IActionResult> Register(SignUpFormData form)
  {
    if (form.Password != form.ConfirmPassword)
    {
      return BadRequest("Passwords do not match.");
    }

    var splitName = form.FullName.Split(" ", 2);
    var userEntity = new UserEntity
    {
      UserName = form.Email,
      Email = form.Email,
      FirstName = splitName.ElementAtOrDefault(0),
      LastName = splitName.ElementAtOrDefault(1)
    };

    var result = await _userManager.CreateAsync(userEntity, form.Password);
    if (!result.Succeeded)
    {
      return BadRequest(result.Errors);
    }

    await _userManager.AddToRoleAsync(userEntity, "User");

    return Ok("User registered successfully.");
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login(SignInFormData form)
  {
    var user = await _userManager.FindByEmailAsync(form.Email);
    if (user == null)
    {
      return Unauthorized("Either the email or password is wrong.");
    }

    var result = await _signInManager.CheckPasswordSignInAsync(user, form.Password, false);
    if (!result.Succeeded)
    {
      return Unauthorized("Either the email or password is wrong.");
    }

    var roles = await _userManager.GetRolesAsync(user);
    var token = GenerateJwtToken(user, roles);

    return Ok(new { token });
  }

  private string GenerateJwtToken(UserEntity user, IList<string> roles)
  {
    var claims = new List<Claim>
    {
      new(ClaimTypes.Name, user.UserName!),
      new(ClaimTypes.NameIdentifier, user.Id),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      issuer: _config["Jwt:Issuer"],
      audience: _config["Jwt:Issuer"],
      claims: claims,
      expires: DateTime.UtcNow.AddDays(3),
      signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}