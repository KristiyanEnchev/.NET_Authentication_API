namespace Persistence.Contexts 
{
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    using Persistence.Constants;

    using Domain.Entities.Identity;
    using System.Data;

    public static class InitialiserExtensions
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

            await initialiser.InitialiseAsync();

            await initialiser.SeedAsync();
        }
    }

    public class ApplicationDbContextInitialiser
    {
        private readonly ILogger<ApplicationDbContextInitialiser> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<UserRole> _roleManager;

        public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<User> userManager, RoleManager<UserRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        public async Task TrySeedAsync()
        {
            var administratorRole = new UserRole(Roles.Administrator);
            var userRole = new UserRole(Roles.User);

            if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await _roleManager.CreateAsync(administratorRole);
            }

            if (_roleManager.Roles.All(r => r.Name != userRole.Name))
            {
                await _roleManager.CreateAsync(userRole);
            }

            var administrator = new User 
            {
                UserName = "admin@admin.com",
                FirstName = "Super",
                LastName = "Admin",
                Email = "admin@admin.com",
                IsActive = true,
                CreatedBy = "Initial Seed",
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = true,
            };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "123456");
                if (!string.IsNullOrWhiteSpace(administratorRole.Name))
                {
                    await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
                }
            }
        }
    }

}