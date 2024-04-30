using Internship_system.BLL.DTOs;
using Internship_system.BLL.Exceptions;
using Internship_system.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController: Controller {
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
        return Ok(await _authService.RegisterAsync(accountRegisterDto));
    }

    /// <summary>
    /// Login user into the system
    /// </summary>
    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<string>> Login([FromBody] AccountLoginDto accountLoginDto) {
        return Ok(await _authService.LoginAsync(accountLoginDto));
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
    
    /// <summary>
    /// Load xls file with students. see file example xz gde.
    /// </summary>
    /// <param name="file"></param>
    /// <exception cref="UnauthorizedException"></exception>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("students/table")]
    public async Task LoadStudentsFile(IFormFile file) {
        if (User.Identity == null || Guid.TryParse(User.Identity.Name, out Guid userId) == false) {
            throw new UnauthorizedException("User is not authorized");
        } 
        await _authService.LoadStudents(file);
    }

    /// <summary>
    /// Get loaded students
    /// </summary>
    /// <exception cref="UnauthorizedException"></exception>
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("students/table")]
    public async Task<List<LoadedStudentDto>> GetLoadedStudents() {
        if (User.Identity == null || Guid.TryParse(User.Identity.Name, out Guid userId) == false) {
            throw new UnauthorizedException("User is not authorized");
        }
        return await _authService.GetLoadedStudents();
    }
    
}