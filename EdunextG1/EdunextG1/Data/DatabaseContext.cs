using EdunextG1.Models;
using Microsoft.EntityFrameworkCore;

namespace EdunextG1.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

    }
}
