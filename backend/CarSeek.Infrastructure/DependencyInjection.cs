using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Infrastructure.Authentication;
using CarSeek.Infrastructure.Services;
using CarSeek.Infrastructure.Persistence;

namespace CarSeek.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration.GetValue<string>("Database:Provider", "Sqlite");
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"));
            }
            else
            {
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            }
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        // Add this line in the AddInfrastructureServices method
        services.AddScoped<IActivityLogger, ActivityLogger>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
