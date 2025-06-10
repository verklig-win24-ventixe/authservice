using Presentation.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Presentation.Domain.Dtos;
using Presentation.Interfaces;

namespace Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, ITokenGenerationService tokenGenerationService, IEmailSenderService emailSenderService) : ControllerBase
{
  private readonly UserManager<UserEntity> _userManager = userManager;
  private readonly SignInManager<UserEntity> _signInManager = signInManager;
  private readonly ITokenGenerationService _tokenGenerationService = tokenGenerationService;
  private readonly IEmailSenderService _emailSenderService = emailSenderService;

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

    var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(userEntity);
    var encodedEmailToken = System.Web.HttpUtility.UrlEncode(emailToken);
    var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = userEntity.Id, emailToken = encodedEmailToken }, Request.Scheme);

    var htmlMessage = $"<p>Hello {userEntity.FirstName},</p>" +
                      $"<p>Please confirm your email by clicking <a href='{confirmationLink}'>here</a>.</p>";

    await _emailSenderService.SendEmailAsync(userEntity.Email, "Confirm your email", htmlMessage);

    return Ok("User registered successfully. Please check your email to verify your account.");
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login(SignInFormData form)
  {
    var user = await _userManager.FindByEmailAsync(form.Email);
    if (user == null)
    {
      return Unauthorized("Either the email or password is wrong.");
    }

    if (!user.EmailConfirmed)
    {
      return Unauthorized("The email was not confirmed, check your inbox and confirm it before logging in.");
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

  [HttpGet("confirm-email")]
  public async Task<IActionResult> ConfirmEmail(string userId, string emailToken)
  {
    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(emailToken))
    {
      return BadRequest("Invalid confirmation link.");
    }

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
      return NotFound("User was not found.");
    }

    var decodedToken = System.Web.HttpUtility.UrlDecode(emailToken);
    var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

    return result.Succeeded
      ? Ok("The email was confirmed successfully, you can now log in.")
      : BadRequest("The email confirmation failed.");
  }
}