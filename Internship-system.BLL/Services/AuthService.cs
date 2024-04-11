using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Internship_system.BLL.DTOs;
using Internship_system.BLL.Exceptions;
using Internship_system.DAL.Data;
using Internship_system.DAL.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Internship_system.BLL.Services;

public class AuthService {
    private readonly ILogger<AuthService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly InterDbContext _interDbContext;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AuthService> logger, InterDbContext interDbContext, IConfiguration configuration) {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _interDbContext = interDbContext;
        _configuration = configuration;
    }

    public async Task<string> RegisterAsync(AccountRegisterDto accountRegisterDto, HttpContext httpContext) {
        if (accountRegisterDto.Email == null) {
            throw new ArgumentNullException(nameof(accountRegisterDto), "Email is empty");
        }

        if (accountRegisterDto.Password == null) {
            throw new ArgumentNullException(nameof(accountRegisterDto), "Password is empty");
        }

        if (await _userManager.FindByEmailAsync(accountRegisterDto.Email) != null) {
            throw new ConflictException("User with this email already exists");
        }

        var user = new User {
            Email = accountRegisterDto.Email,
            UserName = accountRegisterDto.Email,
            FullName = accountRegisterDto.FullName,
        };

        var result = await _userManager.CreateAsync(user, accountRegisterDto.Password);

        if (result.Succeeded) {
            _logger.LogInformation("Successful register");

            return await LoginAsync(new AccountLoginDto()
                { Email = accountRegisterDto.Email, Password = accountRegisterDto.Password }, httpContext);
        }

        var errors = string.Join(", ", result.Errors.Select(x => x.Description));
        throw new BadRequestException(errors);
    }
    public async Task<string> LoginAsync(AccountLoginDto accountLoginDto, HttpContext httpContext) {
        var identity = await GetIdentity(accountLoginDto.Email.ToLower(), accountLoginDto.Password);
        if (identity == null) {
            throw new BadRequestException("Incorrect username or password");
        }
        
        var jwt = new JwtSecurityToken(
            issuer: _configuration.GetSection("Jwt")["Issuer"],
            audience: _configuration.GetSection("Jwt")["Audience"],
            notBefore: DateTime.UtcNow,
            claims: identity.Claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(_configuration.GetSection("Jwt")
                .GetValue<int>("AccessTokenLifetimeInMinutes"))),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(_configuration.GetSection("Jwt")["Secret"] ?? string.Empty)),
                SecurityAlgorithms.HmacSha256));

        _logger.LogInformation("Successful login");

        return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

    public async Task<ProfileDto> GetMyProfile(Guid userId) {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) {
            throw new NotFoundException("User not found");
        }
        var roles = await _userManager.GetRolesAsync(user);
        return new ProfileDto {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            JoinedAt = user.JoinedAt,
            Roles = roles
        };
    }

    private async Task<ClaimsIdentity?> GetIdentity(string email, string password) {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) {
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded) return null;

        var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, user.Id.ToString())
        };

        foreach (var role in await _userManager.GetRolesAsync(user)) {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsIdentity(claims, "Token", ClaimTypes.Name, ClaimTypes.Role);
    }
}