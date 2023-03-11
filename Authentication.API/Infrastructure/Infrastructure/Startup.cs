namespace Infrastructure
{
    using System.Net;
    using System.Text;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using MediatR;

    using Application.Interfaces;

    using Domain.Common;
    using Domain.Entities.Identity;
    using Domain.Common.Interfaces;

    using Infrastructure.Services;
    using Infrastructure.Identity.Services;

    using Persistence.Contexts;
    using Persistence.Constants;

    using Models;

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
                            var errorResponse = new
                            {
                                success = false,
                                data = (object)null,
                                errors = new List<string> { "Authentication failed. Access is denied.", "Unauthorized Access" }
                            };
                            var response = context.Response;
                            response.ContentType = "application/json";
                            response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            await response.WriteAsync(JsonConvert.SerializeObject(errorResponse, new JsonSerializerSettings
                            {
                                ContractResolver = new DefaultContractResolver
                                {
                                    NamingStrategy = new CamelCaseNamingStrategy(true, true)
                                }
                            }));
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