using Microsoft.EntityFrameworkCore;
using systrack_api.Models;

namespace SystrackApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Accounts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Computer> Computers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
    .Property(o => o.PurchaseType)
    .HasConversion(
        v => v.ToString(),
        v => (PurchaseType)Enum.Parse(typeof(PurchaseType), v)
    );

        }
    }
}
