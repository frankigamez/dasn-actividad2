using System.Data.Entity;
using DASN.WebApp.Models.DataModels;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DASN.WebApp.Models
{
    public class DASNDbContext : IdentityDbContext<DataModels.ApplicationUser>
    {
        public DASNDbContext(string connectionStringName = "DASNDB") : base(connectionStringName) { }

        public virtual DbSet<DASNote> DASNotes { get; set; }

        public static DASNDbContext Create() => new DASNDbContext();
    }
}