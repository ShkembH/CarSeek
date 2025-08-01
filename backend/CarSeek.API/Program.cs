using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CarSeek.Application;
using CarSeek.Application.Common.Interfaces;
using CarSeek.Infrastructure;
using CarSeek.API.Middleware;
using CarSeek.Infrastructure.Persistence;
using CarSeek.Infrastructure.Authentication;  // Add this line
using CarSeek.Infrastructure.Services;
using CarSeek.API.src; // For ChatHub

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add CORS services
        // Add CORS services
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Add services to the container.
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   ValidIssuer = builder.Configuration["Jwt:Issuer"],
                   ValidAudience = builder.Configuration["Jwt:Audience"],
                   IssuerSigningKey = new SymmetricSecurityKey(
                       Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ??
                           throw new InvalidOperationException("JWT Key is not configured")))
               };
               // Allow JWT auth for SignalR WebSocket connections
               options.Events = new JwtBearerEvents
               {
                   OnMessageReceived = context =>
                   {
                       var accessToken = context.Request.Query["access_token"];
                       var path = context.HttpContext.Request.Path;
                       if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                       {
                           context.Token = accessToken;
                       }
                       return Task.CompletedTask;
                   }
               };
           });

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            });
        
        // Configure form options for file uploads
        builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.ValueLengthLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = int.MaxValue;
            options.MultipartHeadersLengthLimit = int.MaxValue;
        });
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddApplication();

        // Add SignalR
        builder.Services.AddSignalR();

        // Add Infrastructure services first
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
        builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Conditionally configure the database
        if (builder.Environment.EnvironmentName == "Testing")
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            builder.Services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());
        }
        else
        {
            builder.Services.AddInfrastructure(builder.Configuration);
        }

        var app = builder.Build();

        // Ensure database is created and migrated
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Only create the database if it doesn't exist
            context.Database.EnsureCreated();
            
            // Apply indexes manually if they don't exist
            try
            {
                // Create indexes for better performance
                context.Database.ExecuteSqlRaw(@"
                    CREATE INDEX IF NOT EXISTS IX_CarListing_Status ON CarSeekCarListings (Status);
                    CREATE INDEX IF NOT EXISTS IX_CarListing_Make ON CarSeekCarListings (Make);
                    CREATE INDEX IF NOT EXISTS IX_CarListing_Model ON CarSeekCarListings (Model);
                    CREATE INDEX IF NOT EXISTS IX_CarListing_Year ON CarSeekCarListings (Year);
                    CREATE INDEX IF NOT EXISTS IX_CarListing_Price ON CarSeekCarListings (Price);
                    CREATE INDEX IF NOT EXISTS IX_CarListing_UserId ON CarSeekCarListings (UserId);
                    CREATE INDEX IF NOT EXISTS IX_CarListing_DealershipId ON CarSeekCarListings (DealershipId);
                    CREATE INDEX IF NOT EXISTS IX_CarListing_Status_Make_Model ON CarSeekCarListings (Status, Make, Model);
                    CREATE INDEX IF NOT EXISTS IX_CarListing_Status_Year_Price ON CarSeekCarListings (Status, Year, Price);
                    CREATE INDEX IF NOT EXISTS IX_CarListing_Status_UserId ON CarSeekCarListings (Status, UserId);
                ");
                
                // Create indexes for User table
                context.Database.ExecuteSqlRaw(@"
                    CREATE INDEX IF NOT EXISTS IX_User_Role ON CarSeekUsers (Role);
                    CREATE INDEX IF NOT EXISTS IX_User_Status ON CarSeekUsers (Status);
                    CREATE INDEX IF NOT EXISTS IX_User_IsActive ON CarSeekUsers (IsActive);
                    CREATE INDEX IF NOT EXISTS IX_User_Role_IsActive ON CarSeekUsers (Role, IsActive);
                    CREATE INDEX IF NOT EXISTS IX_User_Status_IsActive ON CarSeekUsers (Status, IsActive);
                ");
                
                // Create indexes for ChatMessage table
                context.Database.ExecuteSqlRaw(@"
                    CREATE INDEX IF NOT EXISTS IX_ChatMessage_SenderId ON ChatMessages (SenderId);
                    CREATE INDEX IF NOT EXISTS IX_ChatMessage_RecipientId ON ChatMessages (RecipientId);
                    CREATE INDEX IF NOT EXISTS IX_ChatMessage_ListingId ON ChatMessages (ListingId);
                    CREATE INDEX IF NOT EXISTS IX_ChatMessage_CreatedAt ON ChatMessages (CreatedAt);
                    CREATE INDEX IF NOT EXISTS IX_ChatMessage_IsRead ON ChatMessages (IsRead);
                    CREATE INDEX IF NOT EXISTS IX_ChatMessage_SenderId_RecipientId_ListingId ON ChatMessages (SenderId, RecipientId, ListingId);
                    CREATE INDEX IF NOT EXISTS IX_ChatMessage_RecipientId_IsRead ON ChatMessages (RecipientId, IsRead);
                    CREATE INDEX IF NOT EXISTS IX_ChatMessage_ListingId_CreatedAt ON ChatMessages (ListingId, CreatedAt);
                ");
                
                Console.WriteLine("✅ Database indexes created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Warning: Could not create indexes: {ex.Message}");
                Console.WriteLine("Indexes may already exist or database doesn't support them");
            }
        }

        // Configure the HTTP request pipeline.
        // Comment out the environment check temporarily
        // if (app.Environment.IsDevelopment())
        // {
        app.UseSwagger();
        app.UseSwaggerUI();
        // }

        // Only use HTTPS redirection in production
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // Add CORS middleware (IMPORTANT: Must be before Authentication/Authorization)
        app.UseCors("AllowReactApp");

        // Add this line before app.UseRouting()
        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.MapControllers();

        // Map SignalR ChatHub
        app.MapHub<ChatHub>("/chathub");

        app.Run();
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
