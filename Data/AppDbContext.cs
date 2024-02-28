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
    }
}
