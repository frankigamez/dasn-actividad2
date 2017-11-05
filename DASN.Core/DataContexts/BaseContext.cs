using System.Data.Entity;
using DASN.Core.Models.DataModels;

namespace DASN.Core.DataContexts
{
    public class BaseContext : DbContext, IContext
    {
        public BaseContext(string connectionStringName = "DASNDB") : base(connectionStringName) { }

        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
    }
}