using Internship_system.BLL.DTOs;
using Internship_system.BLL.Exceptions;
using Internship_system.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController: ControllerBase {
    private readonly AuthService _authService;

    public AuthController(AuthService authService) {
        _authService = authService;
    }

    /// <summary>
    /// Register new user
    /// </summary>
    [HttpPost]
    [Route("register")]
    public async Task<ActionResult<string>> Register([FromBody] AccountRegisterDto accountRegisterDto) {
        return Ok(await _authService.RegisterAsync(accountRegisterDto, HttpContext));
    }

    /// <summary>
    /// Login user into the system
    /// </summary>
    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<string>> Login([FromBody] AccountLoginDto accountLoginDto) {
        return Ok(await _authService.LoginAsync(accountLoginDto, HttpContext));
    }
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("profile")]
    public async Task<ActionResult<ProfileDto>> GetCurrentProfile() {
        if (User.Identity == null || Guid.TryParse(User.Identity.Name, out Guid userId) == false) {
            throw new UnauthorizedException("User is not authorized");
        }
        return Ok(await _authService.GetMyProfile(userId));
    }
    
}