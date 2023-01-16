namespace Application
{
    using System.Reflection;

    using Microsoft.Extensions.DependencyInjection;


    public static class Startup
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper();

        }

        private static void AddAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }

    }
}
