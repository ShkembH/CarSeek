using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Infrastructure.Persistence;
using CarSeek.Infrastructure.Authentication;
using CarSeek.Infrastructure.Services;

namespace CarSeek.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure database
        var databaseProvider = configuration["Database:Provider"] ?? "SqlServer";
        
        if (databaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));
        }

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Configure Redis caching
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "CarSeek_";
        });

        // Register cache service
        services.AddScoped<ICacheService, RedisCacheService>();

        // Register other services
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IActivityLogger, ActivityLogger>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
