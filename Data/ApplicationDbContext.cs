using Microsoft.EntityFrameworkCore;
using Chinhsachso.Models;

namespace Chinhsachso.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Metadata> Metadatas { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
    }
}
