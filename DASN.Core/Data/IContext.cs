using System.Data.Entity;
using DASN.Core.Data.Models;

namespace DASN.Core.Data
{
    public interface IContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Auth> Auths { get; set; }
        DbSet<Post> Posts { get; set; }
        int SaveChanges();
    }
}