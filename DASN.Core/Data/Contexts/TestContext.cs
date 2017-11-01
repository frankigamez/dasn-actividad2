using System.Data.Entity;
using DASN.Core.Data.Models;
using SQLite.CodeFirst;

namespace DASN.Core.Data.Contexts
{
    public class TestDbContext : DbContext, IContext
    {
        public TestDbContext() : base("DASNDBTest") {}

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Auth> Auths { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteDropCreateDatabaseAlways<TestDbContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);           
        }
    } 
}
