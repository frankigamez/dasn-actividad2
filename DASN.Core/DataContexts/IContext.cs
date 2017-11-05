using System.Data.Entity;
using DASN.Core.Models.DataModels;

namespace DASN.Core.DataContexts
{
    public interface IContext
    {
        DbSet<ApplicationUser> ApplicationUsers { get; set; }
        DbSet<Post> Posts { get; set; }
        
        int SaveChanges();
    }
}