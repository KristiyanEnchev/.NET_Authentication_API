namespace Web
{
    using System.Reflection;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class Startup
    {
        public static IServiceCollection AddWeb(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpContextAccessor();
            services.AddControllers().AddApplicationPart(Assembly.GetExecutingAssembly());

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }

        public static IApplicationBuilder UseWeb(this IApplicationBuilder builder)
        {
            builder.UseSwagger()
                    .UseSwaggerUI()
                    .UseStaticFiles()
                    .UseHttpsRedirection()
                    .UseRouting()
                    .UseAuthentication()
                    .UseAuthorization();

            return builder;
        }

        public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapControllers()
                    .RequireAuthorization();

            return builder;
        }
    }
}