using Presentation.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Presentation.Domain.Dtos;
using Presentation.Interfaces;

namespace Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, ITokenGenerationService tokenGenerationService) : ControllerBase
{
  private readonly UserManager<UserEntity> _userManager = userManager;
  private readonly SignInManager<UserEntity> _signInManager = signInManager;
  private readonly ITokenGenerationService _tokenGenerationService = tokenGenerationService;

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
    var token = _tokenGenerationService.GenerateJwtToken(user, roles);

    return Ok(new { token });
  }
}