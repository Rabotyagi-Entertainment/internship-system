using Internship_system.BLL.DTOs;
using Internship_system.BLL.DTOs.Internship.Responses;
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
    public async Task<ActionResult<JwtResponseDto>> Register([FromBody] AccountRegisterDto accountRegisterDto) {
        var jwt = await _authService.RegisterAsync(accountRegisterDto);
        var response = new JwtResponseDto {
            JWT = jwt
        };
        return Ok(response);
    }

    /// <summary>
    /// Login user into the system
    /// </summary>
    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<JwtResponseDto>> Login([FromBody] AccountLoginDto accountLoginDto) {
        var jwt = await _authService.LoginAsync(accountLoginDto);
        var response = new JwtResponseDto {
            JWT = jwt
        };
        return Ok(response);
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
    /// Send deadline message to tg
    /// </summary>
    /// <param name="dto"></param>
    /// <exception cref="UnauthorizedException"></exception>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("deadline/message")]
    public async Task SendDeadlineMessage([FromBody] DeadlineMessageDto dto) {
        if (User.Identity == null || Guid.TryParse(User.Identity.Name, out Guid userId) == false) {
            throw new UnauthorizedException("User is not authorized");
        } 
        await _authService.SendDeadlineMessage(dto);
    }

    /// <summary>
    /// Get loaded students
    /// </summary>
    /// <exception cref="UnauthorizedException"></exception>
    [HttpGet]
    [Route("students/table")]
    public async Task<List<LoadedStudentDto>> GetLoadedStudents() {
        return await _authService.GetLoadedStudents();
    }
    
    
}