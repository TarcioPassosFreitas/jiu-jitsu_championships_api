using FluentValidation;
using JiuJitsuMaster.Core.Interfaces.Services;
using JiuJitsuMaster.Core.Services;
using JiuJitsuMaster.Core.Validators;
using JiuJitsuMaster.Core.Validators.AthleteValidator;
using Microsoft.Extensions.DependencyInjection;

namespace JiuJitsuMaster.Core;

public static class CoreInjector
{
    public static void RegisterCoreServices(this IServiceCollection services)
    {
        RegisterServices(services);
        RegisterValidators(services);
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IMailKitService, MailKitService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRecoverPasswordService, UserRecoverPasswordService>();
        services.AddScoped<IChampionshipService, ChampionshipService>();
        services.AddScoped<IGitHubStorageService, GitHubStorageService>();
        services.AddScoped<IAthleteService, AthleteService>();
        services.AddScoped<ICaptchaService, CaptchaService>();
    }

    private static void RegisterValidators(IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<UserValidator>();
        services.AddValidatorsFromAssemblyContaining<UserUpdateValidator>();
        services.AddValidatorsFromAssemblyContaining<ChampionshipValidator>();
        services.AddValidatorsFromAssemblyContaining<AddAthleteValidator>();
    }
}