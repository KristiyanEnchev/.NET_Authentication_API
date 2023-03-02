namespace Persistence
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Persistence.Contexts;
    using Persistence.Repositories;

    using Application.Interfaces.Repositories;

    public static class Startup
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext(configuration);
            services.AddRepositories();

            return services;
        }

        public static void AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(connectionString,
                   builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<ApplicationDbContextInitialiser>();
        }
        private static void AddRepositories(this IServiceCollection services)
        {
            services
                .AddTransient(typeof(IBaseRepository), typeof(BaseRepository))
                .AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        }
    }
}