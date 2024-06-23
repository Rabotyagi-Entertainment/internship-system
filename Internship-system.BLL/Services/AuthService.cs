using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Internship_system.BLL.DTOs;
using Internship_system.BLL.Exceptions;
using internship_system.Common.Enums;
using Internship_system.DAL.Configuration;
using Internship_system.DAL.Data;
using Internship_system.DAL.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = Internship_system.DAL.Data.Entities.User;

namespace Internship_system.BLL.Services;

public class AuthService {
    private readonly ILogger<AuthService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly InterDbContext _interDbContext;
    private readonly IConfiguration _configuration;
    private readonly ITelegramBotClient _telegramBot;

    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager,
        ILogger<AuthService> logger, InterDbContext interDbContext, 
        IConfiguration configuration, ITelegramBotClient telegramBot) {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _interDbContext = interDbContext;
        _configuration = configuration;
        _telegramBot = telegramBot;
    }

    public async Task SendDeadlineMessage(DeadlineMessageDto dto) {
        var students = _interDbContext.Students
            .Where(s => s.CourseNumber == dto.CourseNumber);
        var studentUserNames = await students
            .Select(s => s.UserName)
            .ToListAsync();

        var tgStudents = await _interDbContext.StudentTelegrams
            .Where(stg => studentUserNames
                .Contains(stg.TgName))
            .ToListAsync();
        foreach (var studentTelegram in tgStudents) {
            await _telegramBot.SendTextMessageAsync(new ChatId($"{studentTelegram.ChatId}"),
                $"Внимание! Скоро дедлайн по дневнику практики. Нужно заполнить до: {dto.DeadlineTime} \n {dto.OptionalMessage}");
        }
    }

    public async Task LoadStudents(IFormFile file) {
        
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        var students = new List<StudentInfo>();
        for (var row = 2; row <= worksheet.Dimension.Rows; row++) {
            var isNumber = int.TryParse(worksheet.Cells[row, 3].Text, out var courseNumber);
            if (worksheet.Cells[row, 1].Text.IsNullOrEmpty() ||
                worksheet.Cells[row, 2].Text.IsNullOrEmpty() ||
                worksheet.Cells[row, 3].Text.IsNullOrEmpty())
                continue;
            var student = new StudentInfo {
                FullName = worksheet.Cells[row, 1].Text ?? "НЕ ЗАПОЛНЕНО",
                CourseNumber = isNumber ? courseNumber : null,
                Group = worksheet.Cells[row, 2].Text ?? "НЕ ЗАПОЛНЕНО",
            };
            students.Add(student);
        }

        var duplicates = await _interDbContext.StudentInfos
            .Where(si => students
                .Select(s=>s.FullName)
                .Contains(si.FullName))
            .ToListAsync();
        
        if (duplicates.Count != 0)
            throw new ConflictException("These students already exist: " + string.Join("\n", duplicates));
        
        if (students.Count > 0) {
            _interDbContext.StudentInfos.AddRange(students);
            await _interDbContext.SaveChangesAsync();
        }
    }

    public async Task<string> RegisterAsync(AccountRegisterDto accountRegisterDto) {
        if (accountRegisterDto.Password == null) {
            throw new ArgumentNullException(nameof(accountRegisterDto), "Password is empty");
        }
        
        if (accountRegisterDto.TelegramUserName == null) {
            throw new ArgumentNullException(nameof(accountRegisterDto), "tg name is empty");
        }

        if (await _userManager.FindByNameAsync(accountRegisterDto.TelegramUserName) != null) {
            throw new ConflictException("User with this tg name already exists");
        }

        var studentInfo = await _interDbContext.StudentInfos
            .FirstOrDefaultAsync(si => si.FullName == accountRegisterDto.FullName);
        if (studentInfo == null)
            throw new NotFoundException("There is no students with this fullName");
        if (studentInfo.AttachedAt.HasValue)
            throw new ForbiddenException("Student with this full name already registered");
        
        var user = new Student() {
            UserName = accountRegisterDto.TelegramUserName,
            Email = accountRegisterDto.Email,
            FullName = accountRegisterDto.FullName,
            CourseNumber = studentInfo.CourseNumber,
            Group = studentInfo.Group
        };

        var result = await _userManager.CreateAsync(user, accountRegisterDto.Password);

        if (result.Succeeded) {
            _logger.LogInformation("Successful register");
            
            studentInfo.AttachedAt = DateTime.UtcNow;
            _interDbContext.Update(studentInfo);
            await _interDbContext.SaveChangesAsync();
            
            var createdUser = await _userManager
                .FindByIdAsync(user.Id.ToString());

            await _userManager.AddToRoleAsync(createdUser!, ApplicationRoleNames.Student);
            
            return await LoginAsync(new AccountLoginDto()
                { TelegramUserName = accountRegisterDto.TelegramUserName, Password = accountRegisterDto.Password });
        }

        var errors = string.Join(", ", result.Errors.Select(x => x.Description));
        throw new BadRequestException(errors);
    }

    public async Task<List<LoadedStudentDto>> GetLoadedStudents() {
        var students = await _interDbContext.StudentInfos
            .Take(100)
            .ToListAsync();
        return students.Select(s => new LoadedStudentDto {
            Id = s.Id,
            FullName = s.FullName,
            Group = s.Group,
            CourseNumber = s.CourseNumber
        }).ToList();
    }
    public async Task<string> LoginAsync(AccountLoginDto accountLoginDto) {
        var identity = await GetIdentity(accountLoginDto.TelegramUserName.ToLower(), accountLoginDto.Password);
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
            TelegramUserName = user.UserName,
            Roles = roles
        };
    }

    private async Task<ClaimsIdentity?> GetIdentity(string userName, string password) {
        var user = await _userManager.FindByNameAsync(userName);
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