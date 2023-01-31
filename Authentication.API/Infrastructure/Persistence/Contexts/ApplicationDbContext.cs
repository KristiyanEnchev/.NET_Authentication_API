namespace Persistence.Contexts
{
    using System.Reflection;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

    using Application.Interfaces;

    using Domain.Common;
    using Domain.Common.Interfaces;
    using Domain.Entities.Identity;

    public class ApplicationDbContext :
        IdentityDbContext<
            User,
            UserRole,
            string,
            IdentityUserClaim<string>,
            IdentityUserRole<string>,
            IdentityUserLogin<string>,
            RoleClaim,
            IdentityUserToken<string>>
    {
        private readonly IDomainEventDispatcher _dispatcher;
        private readonly IUser _user;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
          IDomainEventDispatcher dispatcher,
          IUser user)
            : base(options)
        {
            _dispatcher = dispatcher;
            _user = user;
        }

        public DbSet<UserRole> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            DisableCascadeDelete(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (_dispatcher == null) return result;

            await DispatchEvent();

            if (_user == null) return result;

            UpdateEntities();

            return result;
        }

        public override int SaveChanges()
        {
            return SaveChangesAsync().GetAwaiter().GetResult();
        }

        public async Task DispatchEvent()
        {
            var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToArray();

            await _dispatcher.DispatchAndClearEvents(entitiesWithEvents);
        }

        public void UpdateEntities()
        {
            foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _user.Id;
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified ||
                    entry.References.Any(r =>
                        r.TargetEntry != null &&
                        r.TargetEntry.Metadata.IsOwned() &&
                        (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified)))
                {
                    entry.Entity.UpdatedBy = _user.Id;
                    entry.Entity.UpdatedDate = DateTime.UtcNow;
                }
            }
        }

        public static void DisableCascadeDelete(ModelBuilder builder)
        {
            var entityTypes = builder.Model.GetEntityTypes().ToList();
            var foreignKeys = entityTypes
            .SelectMany(e => e.GetForeignKeys().Where(f => f.DeleteBehavior == DeleteBehavior.Cascade));
            foreach (var foreignKey in foreignKeys)
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}