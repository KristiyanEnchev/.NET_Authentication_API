namespace Infrastructure
{
    using System.Text;

    using Microsoft.IdentityModel.Tokens;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    using MediatR;

    using Application.Interfaces;

    using Domain.Common;
    using Domain.Entities.Identity;
    using Domain.Common.Interfaces;

    using Infrastructure.Services;

    using Persistence.Contexts;

    using Models;
    using Persistence.Constants;

    public static class Startup
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddServices(configuration);
            services.AddIdentity(configuration);

            return services;
        }

        private static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddTransient<IMediator, Mediator>()
                .AddTransient<IDomainEventDispatcher, DomainEventDispatcher>()
                .AddTransient<IDateTimeService, DateTimeService>();
        }

        private static void AddIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddIdentity<User, UserRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddTokenProvider("Authentication.Api", typeof(DataProtectorTokenProvider<User>));
        }
        private static void AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApplicationSettings>(configuration.GetSection(nameof(ApplicationSettings)));
        }


        private static void AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var secret = configuration
                .GetSection(nameof(ApplicationSettings))
                .GetValue<string>(nameof(ApplicationSettings.Secret))!;

            var key = Encoding.UTF8.GetBytes(secret);

            services
                .AddAuthentication(authentication =>
                {
                    authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(bearer =>
                {
                    bearer.RequireHttpsMetadata = false;
                    bearer.SaveToken = true;
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
        }

        private static void AddCustomAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthorization(options =>
                options.AddPolicy(Policies.CanDelete, policy => policy.RequireRole(Roles.Administrator)));
        }
    }
}
