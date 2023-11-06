using JiuJitsuMaster.Core.Interfaces.Repositories;
using JiuJitsuMaster.Infrastructure.Options;
using JiuJitsuMaster.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JiuJitsuMaster.Infrastructure
{
    public static class InfrastructureInjector
    {
        public static IServiceCollection RegisterMailKit(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MailKitOptions>(configuration.GetSection("MailKitOptions"));
            services.AddScoped<IMailKitRepository, MailKitRepository>();
            return services;
        }

        public static IServiceCollection RegisterInfrastructure(this IServiceCollection services)
        {
            RegisterRepositories(services);
            return services;
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped<IMailKitRepository, MailKitRepository>();
            services.AddScoped<IUserCreatePasswordRepository, UserCreatePasswordRepository>();
            services.AddScoped<IUserRecoverPasswordRepository, UserRecoverPasswordRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IChampionshipRepository, ChampionshipRepository>();
            services.AddScoped<IGitHubStorageRepository, GitHubStorageRepository>();
            services.AddScoped<IAthleteRepository, AthleteRepository>();
            services.AddScoped<IAthleteChampionshipRepository, AthleteChampionshipRepository>();
            services.AddScoped<IAthleteFightRepository, AthleteFightRepository>();
            services.AddScoped<IFightRepository, FightRepository>();
            services.AddScoped<IAthleteResultRepository, AthleteResultRepository>();
            services.AddScoped<IResultRepository, ResultRepository>();
            services.AddScoped<ICaptchaRepository, CaptchaRepository>();
        }

        public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterRepositories(services);
            services.RegisterMailKit(configuration);
            services.AddLogging(configure => configure.AddConsole())
            .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);
            return services;
        }
    }
}