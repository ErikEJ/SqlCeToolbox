using System.Data.EntityClient;
using System.IO;
using System.Configuration;
using System.Xml;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal class AppConfigHelper
    {
        internal static void WriteConnectionStringToAppConfig(string connectionName, string connectionString, string projectPath, string provider, string prefix, string itemName)
        {
            bool connectionFound = false;

            //http://social.msdn.microsoft.com/forums/en-US/winforms/thread/3943ec30-8be5-4f12-9667-3b812f711fc9/
            if (projectPath == null) return;
            var projectDir = Path.GetDirectoryName(projectPath);
            var itemWithoutExtension = Path.GetFileNameWithoutExtension(itemName);
            if (string.IsNullOrEmpty(itemWithoutExtension)) return;
            if (string.IsNullOrEmpty(projectDir)) return;

            File.WriteAllText(Path.Combine(projectDir, prefix), string.Empty);
            var config = ConfigurationManager.OpenExeConfiguration(Path.Combine(projectDir, itemWithoutExtension));
            try
            {
                File.Delete(Path.Combine(projectDir, "App"));
            }
            catch (IOException)
            { }
            foreach (ConnectionStringSettings connection in config.ConnectionStrings.ConnectionStrings)
            {
                if (connection.Name == connectionName)
                    connectionFound = true;
            }
            if (connectionFound)
                config.ConnectionStrings.ConnectionStrings.Remove(connectionName);

            ConnectionStringSettings csSettings = new ConnectionStringSettings
            {
                Name = connectionName,
                ConnectionString = connectionString,
                ProviderName = provider
            };
            // Get the connection strings section. 
            ConnectionStringsSection csSection =
                config.ConnectionStrings;
            // Add the new element. 
            csSection.ConnectionStrings.Add(csSettings);
            // Save the configuration file. 
            config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
