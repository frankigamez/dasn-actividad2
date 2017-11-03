using System.Data.Entity;
using DASN.Core.Data.Models;

namespace DASN.Core.Data.Contexts
{
    public class Context : DbContext, IContext
    {
        public Context() : base("DASNDB") { }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Auth> Auths { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
    }
}
