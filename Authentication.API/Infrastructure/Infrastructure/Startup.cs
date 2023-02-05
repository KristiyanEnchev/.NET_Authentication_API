namespace Infrastructure
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using MediatR;

    using Application.Interfaces;

    using Domain.Common;
    using Domain.Entities.Identity;
    using Domain.Common.Interfaces;

    using Infrastructure.Services;

    using Persistence.Contexts;

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
    }
}
