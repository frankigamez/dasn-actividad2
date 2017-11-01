using System.Data.Entity;
using DASN.Data.Model;

namespace DASN.Data.Context
{
    public class DbContext : System.Data.Entity.DbContext, IContext
    {
        public DbContext() : base("DASNDB") { }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Auth> Auths { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
    }
}
