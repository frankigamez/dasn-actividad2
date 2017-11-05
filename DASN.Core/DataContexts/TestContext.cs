using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using SQLite.CodeFirst;

namespace DASN.Core.DataContexts
{
    public class TestDbContext : BaseContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserLogin>().HasKey(l => l.UserId);
            modelBuilder.Entity<IdentityRole>().HasKey(r => r.Id);
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId });

            var sqliteConnectionInitializer = new SqliteDropCreateDatabaseAlways<TestDbContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);           
        }
    } 
}
