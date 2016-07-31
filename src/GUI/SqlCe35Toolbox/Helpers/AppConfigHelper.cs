using System.Data.EntityClient;
using System.IO;
using System.Configuration;
using System.Xml;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal class AppConfigHelper
    {
        internal static void BuildEfConfig(string connectionString, string projectPath, string provider, string model, string prefix, string itemName)
        {
            EntityConnectionStringBuilder builder = new EntityConnectionStringBuilder
            {
                Metadata = string.Format("res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", model),
                Provider = provider,
                ProviderConnectionString = connectionString
            };

            var connName = string.Format("{0}Entities", model);

            WriteConnectionStringToAppConfig(connName, builder.ConnectionString, projectPath, "System.Data.EntityClient", prefix, itemName);
        }

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

        internal static void WriteSettings(string configPath, DatabaseType dbType)
        {
            
            if (!File.Exists(configPath))
                return;
            XmlDocument doc;
            doc = new XmlDocument();
            doc.Load(configPath);

            string invariantName = "System.Data.SqlServerCe.4.0";
            if (dbType == DatabaseType.SQLCE35)
            {
                invariantName = "System.Data.SqlServerCe.3.5";
            }

            XmlNode nodeRemoveExists = doc.SelectSingleNode(string.Format("//system.data/DbProviderFactories/remove[@invariant = '{0}']", invariantName));
            XmlNode nodeAddExists = doc.SelectSingleNode(string.Format("//system.data/DbProviderFactories/add[@invariant = '{0}']", invariantName));

            if (nodeAddExists == null && nodeRemoveExists == null)
            {
                bool addDataNode = false;
                bool addProvNode = false;

                XmlNode nodeData = doc.SelectSingleNode("//system.data");
                if (nodeData == null)
                {
                    addDataNode = true;
                    nodeData = doc.CreateNode(XmlNodeType.Element, "system.data", null);
                }
            
                XmlNode nodeProv = nodeData.SelectSingleNode("//DbProviderFactories");
                if (nodeProv == null)
                {
                    addProvNode = true;
                    nodeProv = doc.CreateNode(XmlNodeType.Element, "DbProviderFactories", null);
                }

                XmlElement elem = doc.CreateElement("remove");
                if (dbType == DatabaseType.SQLCE40)
                {
                    elem.SetAttribute("invariant", "System.Data.SqlServerCe.4.0");
                }
                if (dbType == DatabaseType.SQLCE35)
                {
                    elem.SetAttribute("invariant", "System.Data.SqlServerCe.3.5");
                }
                nodeProv.AppendChild(elem);

                XmlElement elemAdd = doc.CreateElement("add");
                if (dbType == DatabaseType.SQLCE40)
                {
                    elemAdd.SetAttribute("name", "Microsoft SQL Server Compact Data Provider 4.0");
                    elemAdd.SetAttribute("invariant", "System.Data.SqlServerCe.4.0");
                    elemAdd.SetAttribute("description", ".NET Framework Data Provider for Microsoft SQL Server Compact");
                    elemAdd.SetAttribute("type", "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Version=4.0.0.1, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                }
                if (dbType == DatabaseType.SQLCE35)
                {
                    elemAdd.SetAttribute("name", "Microsoft SQL Server Compact Data Provider 3.5");
                    elemAdd.SetAttribute("invariant", "System.Data.SqlServerCe.3.5");
                    elemAdd.SetAttribute("description", ".NET Framework Data Provider for Microsoft SQL Server Compact");
                    elemAdd.SetAttribute("type", "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Version=3.5.1.50, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                }
                nodeProv.AppendChild(elemAdd);

                nodeData.AppendChild(nodeProv);

                if (!addDataNode)
                {
                    XmlNode nodeSystemData = doc.SelectSingleNode("//system.data");
                    if (nodeSystemData != null)
                        nodeSystemData.AppendChild(nodeProv);
                }
                else if (!addProvNode)
                {
                    XmlNode nodeProvFact = doc.SelectSingleNode("//DbProviderFactories");
                    if (nodeProvFact != null)
                    {
                        nodeProvFact.AppendChild(elem);
                        nodeProvFact.AppendChild(elemAdd);
                    }
                }
                else
                {
                    XmlNode nodeConfig = doc.SelectSingleNode("//configuration");
                    if (nodeConfig != null)
                        nodeConfig.AppendChild(nodeData);
                }
            }

            XmlNode nodeAB;
            nodeAB = doc.SelectSingleNode("//assemblyBinding");
            if (dbType == DatabaseType.SQLCE35)
            {
                XmlNode nodeAiExists = doc.SelectSingleNode(string.Format("//runtime/assemblyBinding/dependentAssembly/assemblyIdentity[@name = '{0}']", "System.Data.SqlServerCe"));
                XmlNode nodeBrExists = doc.SelectSingleNode(string.Format("//runtime/assemblyBinding/dependentAssembly/bindingRedirect[@oldVersion = '{0}']", "3.5.1.0-3.5.1.50"));

                if (nodeAiExists == null && nodeBrExists == null)
                {
                    bool addRtNode = false;
                    bool addAbNode = false;

                    XmlNode nodeRuntime = doc.SelectSingleNode("//runtime");
                    if (nodeRuntime == null)
                    {
                        addRtNode = true;
                        nodeRuntime = doc.CreateNode(XmlNodeType.Element, "runtime", null);
                    }

                    if (nodeAB == null)
                    {
                        addAbNode = true;
                        nodeAB = doc.CreateNode(XmlNodeType.Element, "assemblyBinding", "urn:schemas-microsoft-com:asm.v1");
                    }

                    XmlNode nodeDa = doc.SelectSingleNode("//dependentAssembly") ??
                                     doc.CreateNode(XmlNodeType.Element, "dependentAssembly", null);

                    XmlElement elemId = doc.CreateElement("assemblyIdentity");
                    elemId.SetAttribute("name", "System.Data.SqlServerCe");
                    elemId.SetAttribute("publicKeyToken", "89845dcd8080cc91");
                    elemId.SetAttribute("culture", "neutral");

                    XmlElement elemBr = doc.CreateElement("bindingRedirect");
                    elemBr.SetAttribute("oldVersion", "3.5.1.0-3.5.1.50");
                    elemBr.SetAttribute("newVersion", "3.5.1.50");

                    nodeDa.AppendChild(elemId);
                    nodeDa.AppendChild(elemBr);

                    nodeAB.AppendChild(nodeDa);

                    nodeRuntime.AppendChild(nodeAB);

                    if (!addRtNode)
                    {
                        XmlNode nodeRunTime = doc.SelectSingleNode("//runtime");
                        if (nodeRunTime != null)
                            nodeRunTime.AppendChild(nodeAB);
                    }
                    else if (!addAbNode)
                    {
                        XmlNode nodeAb = doc.SelectSingleNode("//assemblyBinding");
                        if (nodeAb != null)
                            nodeAb.AppendChild(nodeDa);
                    }
                    else
                    {
                        XmlNode nodeConfig = doc.SelectSingleNode("//configuration");
                        if (nodeConfig != null)
                            nodeConfig.AppendChild(nodeRuntime);
                    }
                }
                
            }
            //<runtime>
            //  <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
            //    <dependentAssembly>
            //      <assemblyIdentity name="System.Data.SqlServerCe" publicKeyToken="89845dcd8080cc91" culture="neutral"/>
            //      <bindingRedirect oldVersion="3.5.1.0-3.5.1.50" newVersion="3.5.1.50"/>
            //    </dependentAssembly>
            //  </assemblyBinding>
            //</runtime>
            doc.Save(configPath);
        }
    }
}
