using dotenv.net;
using JiuJitsuMaster.Infrastructure.Database.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using JiuJitsuMaster.AdminAPI.Mappers;
using JiuJitsuMaster.Core;
using JiuJitsuMaster.Infrastructure;
using JiuJitsuMaster.Infrastructure.Options;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/logs-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { ".env" }));

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JiuJitsu API", Version = "v1" });
    c.UseAllOfToExtendReferenceSchemas();
    c.UseInlineDefinitionsForEnums();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("SECRET_ACCESS_TOKEN"));
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddHealthChecks();

var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(DomainToModel));
builder.Services.RegisterCoreServices();
builder.Services.RegisterInfrastructure(builder.Configuration);

var mailKitOptions = new MailKitOptions
{
    HostAddress = Environment.GetEnvironmentVariable("MAILKIT_HOST_ADDRESS"),
    HostPort = int.Parse(Environment.GetEnvironmentVariable("MAILKIT_HOST_PORT") ?? "587"),
    UseSsl = bool.Parse(Environment.GetEnvironmentVariable("MAILKIT_USE_SSL") ?? "true"),
    HostUsername = Environment.GetEnvironmentVariable("MAILKIT_HOST_USERNAME"),
    HostPassword = Environment.GetEnvironmentVariable("MAILKIT_HOST_PASSWORD"),
    SenderName = Environment.GetEnvironmentVariable("MAILKIT_SENDER_NAME"),
    SenderEmail = Environment.GetEnvironmentVariable("MAILKIT_SENDER_EMAIL")
};
builder.Services.AddSingleton(mailKitOptions);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ADMIN API v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();