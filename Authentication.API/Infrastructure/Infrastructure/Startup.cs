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
    using Persistence.Constants;

    using Models;
    using Infrastructure.Identity.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using System.Net;
    using Newtonsoft.Json;

    public static class Startup
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddServices()
                .AddIdentity()
                .AddConfigurations(configuration)
                .AddCustomAuthentication(configuration)
                .AddCustomAuthorization();

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services
                .AddTransient<IMediator, Mediator>()
                .AddTransient<IDomainEventDispatcher, DomainEventDispatcher>()
                .AddTransient<IDateTimeService, DateTimeService>();

            return services;
        }

        private static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            services
                .AddTransient<IIdentity, IdentityService>()
                .AddTransient<IJwtGenerator, JwtGeneratorService>()
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

            return services;
        }
        private static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApplicationSettings>(configuration.GetSection(nameof(ApplicationSettings)));
            return services;
        }

        private static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
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

                    bearer.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse(); 
                            var errorResult = new 
                            {
                                StatusCode = (int)HttpStatusCode.Unauthorized,
                                Messages = new List<string> { "Authentication failed. Access is denied." },
                                Exception = "Unauthorized Access"
                            };
                            var response = context.Response;
                            response.ContentType = "application/json";
                            response.StatusCode = errorResult.StatusCode;
                            await response.WriteAsync(JsonConvert.SerializeObject(errorResult));
                        }
                    };
                });
            return services;
        }

        private static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
                options.AddPolicy(Policies.CanDelete, policy => policy.RequireRole(Roles.Administrator)));

            return services;
        }
    }
}
