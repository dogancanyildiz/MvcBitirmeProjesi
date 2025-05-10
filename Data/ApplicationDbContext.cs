using Microsoft.EntityFrameworkCore;
using MvcBitirmeProjesi.Models;

namespace MvcBitirmeProjesi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<QrLog> QrLogs { get; set; }
    }
}