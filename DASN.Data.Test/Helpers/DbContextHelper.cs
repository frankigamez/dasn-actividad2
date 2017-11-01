using System.Configuration;

namespace DASN.Data.Test.Helpers
{
    internal class DbContextHelper
    {
        private const string Dbfile = "test.db";

        public static void StartContext()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(
                name: "DASNDBTest",
                connectionString: $"Data Source={Dbfile};Version=3;New=True;",
                providerName: "System.Data.SQLite"));
            config.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("connectionStrings");            
        }

        public static void EndContext()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings.Remove(name: "DASNDBTest");
            config.Save(ConfigurationSaveMode.Modified, true);
            ConfigurationManager.RefreshSection("connectionStrings");
        }
    }
}
