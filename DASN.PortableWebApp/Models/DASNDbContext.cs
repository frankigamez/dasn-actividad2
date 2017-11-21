using DASN.PortableWebApp.Models.DataModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DASN.PortableWebApp.Models
{
    public class DASNDbContext : IdentityDbContext<ApplicationUser>
    {
        public DASNDbContext(DbContextOptions<DASNDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DASNote> DASNotes { get; set; }

        public static DASNDbContext Create() => new DASNDbContext(new DbContextOptions<DASNDbContext>());
    }
}