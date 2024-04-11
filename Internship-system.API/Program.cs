using System.Reflection;
using System.Text.Json.Serialization;
using Internship_system.BLL.Exceptions;
using Internship_system.BLL.Extensions;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(
        policy => {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add services to the container.

builder.Services.AddIdentityManagers(builder.Configuration);

builder.Services.AddControllers().AddJsonOptions(opts => {
    var enumConverter = new JsonStringEnumConverter();
    opts.JsonSerializerOptions.Converters.Add(enumConverter);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option => {
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "HW: Account-component", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    option.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddAuthorization();
builder.Services.AddJwtAuthentication(builder.Configuration);

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

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

app.Run();