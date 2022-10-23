using Core.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Core.Infrastructure.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Share> Shares { get; set; }
        public DbSet<UserDisk> UserDisks { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            // Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Share>()
                .HasIndex(x => new { x.UserId, x.Disk, x.Path })
                .IsUnique();

            builder.Entity<Share>()
                .HasIndex(x => x.PublicId)
                .IsUnique();

            builder.Entity<UserDisk>()
                .HasIndex(x => new { x.UserId, x.Disk })
                .IsUnique();
        }

        public UserDisk GetUserDisk(string userId, string diskName)
        {
            return this.UserDisks.Where(x => x.UserId == userId && x.Disk == diskName).First();
        }

        public Share? GetShare(string publicId)
        {
            return this.Shares.Where(x => x.PublicId == publicId).FirstOrDefault();
        }
    }
}
