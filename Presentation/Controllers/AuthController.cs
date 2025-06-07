using Presentation.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Presentation.Controllers;

[AllowAnonymous]
[Route("")]
public class AuthController(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, IConfiguration config) : Controller
{
  private readonly UserManager<UserEntity> _userManager = userManager;
  private readonly SignInManager<UserEntity> _signInManager = signInManager;
  private readonly IConfiguration _config = config;
}