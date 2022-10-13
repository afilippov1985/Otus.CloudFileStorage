using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Common.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Share> Shares { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            // Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Share>()
                .HasIndex(x => new { x.Disk, x.Path })
                .IsUnique();

            builder.Entity<Share>()
                .HasIndex(x => x.PublicId)
                .IsUnique();

            builder.Entity<Share>()
            .HasIndex(x => x.UserId);
        }
    }
}
