using System.Text.Json.Serialization;
using Internship_system.BLL.Exceptions;
using Internship_system.BLL.Extensions;
using Internship_system.BLL.Services;
using Internship_system.Configuration;
using Serilog;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddIdentityManagers(builder.Configuration);

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    var enumConverter = new JsonStringEnumConverter();
    opts.JsonSerializerOptions.Converters.Add(enumConverter);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Services.AddAuthorization();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.ConfigureLogging();

var app = builder.Build();

await app.MigrateDbAsync();
await app.ConfigureIdentity();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseErrorHandleMiddleware();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();
/*var tgService = app.Services.GetRequiredService<TgService>();
await tgService.Execute();*/
app.Run();