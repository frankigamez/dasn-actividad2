using System.Configuration;
using System.Linq;
using DASN.Core.DataContexts;

namespace DASN.Core.Test.Helpers
{
    internal static class TestDbContextHelper
    {
        private const string Dbfile = "test.db";

        public static void StartContext()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(
                name: "DASNDB",
                connectionString: $"Data Source={Dbfile};Version=3;New=True;",
                providerName: "System.Data.SQLite"));
            config.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("connectionStrings");  
        }

        public static void CureContext(this TestDbContext dbContext)
        {
            dbContext.Database.SqlQuery<string>(
                    "select 'delete from ' || name || ';' from sqlite_master  where type = 'table';")
                .ToList().ForEach(tableDropScript =>
                {
                    dbContext.Database.ExecuteSqlCommand(tableDropScript);
                });
        }

        public static void EndContext()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings.Remove(name: "DASNDB");
            config.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("connectionStrings");
        }
    }
}
