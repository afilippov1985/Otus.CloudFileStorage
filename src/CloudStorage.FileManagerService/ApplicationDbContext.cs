using System.Data.Entity;
using CloudStorage.FileManagerService.DAL.Models;

namespace CloudStorage.FileManagerService
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserAuth> Users { get; set; }

        public ApplicationDbContext() : base("Host=localhost;Database=users_auth_db;Username=postgres;Password=123passwd;Persist Security Info=True;")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
            modelBuilder.Entity<UserAuth>().ToTable("users_auth");
        }
    }
}
