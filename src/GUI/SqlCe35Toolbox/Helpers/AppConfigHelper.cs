using System.Data.EntityClient;
using System.IO;
using System.Configuration;
using System.Xml;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal class AppConfigHelper
    {
        internal static void BuildConfig(string connectionString, string projectPath, string provider, string model, string prefix, string itemName)
        {
            EntityConnectionStringBuilder builder = new EntityConnectionStringBuilder();
            builder.Metadata = string.Format("res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", model);
            builder.Provider = provider;
            builder.ProviderConnectionString = connectionString;

            bool connectionFound = false;
            string connName = string.Format("{0}Entities", model);

            //http://social.msdn.microsoft.com/forums/en-US/winforms/thread/3943ec30-8be5-4f12-9667-3b812f711fc9/
            File.WriteAllText(Path.Combine(System.IO.Path.GetDirectoryName(projectPath), prefix), string.Empty);

            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(Path.Combine(System.IO.Path.GetDirectoryName(projectPath), Path.GetFileNameWithoutExtension(itemName)));
            try
            {
                File.Delete(Path.Combine(System.IO.Path.GetDirectoryName(projectPath), "App"));
            }
            catch (IOException)
            { }
            foreach (ConnectionStringSettings connection in config.ConnectionStrings.ConnectionStrings)
            {
                if (connection.Name == connName)
                    connectionFound = true;
            }
            if (connectionFound)
                config.ConnectionStrings.ConnectionStrings.Remove(string.Format("{0}Entities", model));

            ConnectionStringSettings csSettings = new ConnectionStringSettings();
            csSettings.Name = connName;
            csSettings.ConnectionString = builder.ConnectionString;
            csSettings.ProviderName = "System.Data.EntityClient";
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
            XmlDocument doc = null;
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

            if (dbType == DatabaseType.SQLCE35)
            {
                XmlNode nodeAIExists = doc.SelectSingleNode(string.Format("//runtime/assemblyBinding/dependentAssembly/assemblyIdentity[@name = '{0}']", "System.Data.SqlServerCe"));
                XmlNode nodeBRExists = doc.SelectSingleNode(string.Format("//runtime/assemblyBinding/dependentAssembly/bindingRedirect[@oldVersion = '{0}']", "3.5.1.0-3.5.1.50"));

                if (nodeAIExists == null && nodeBRExists == null)
                {

                    bool addRtNode = false;
                    bool addABNode = false;

                    XmlNode nodeRuntime = doc.SelectSingleNode("//runtime");
                    if (nodeRuntime == null)
                    {
                        addRtNode = true;
                        nodeRuntime = doc.CreateNode(XmlNodeType.Element, "runtime", null);
                    }

                    XmlNode nodeAB = doc.SelectSingleNode("//assemblyBinding");
                    if (nodeAB == null)
                    {
                        addABNode = true;
                        nodeAB = doc.CreateNode(XmlNodeType.Element, "assemblyBinding", "urn:schemas-microsoft-com:asm.v1");
                    }

                    XmlNode nodeDA = doc.SelectSingleNode("//dependentAssembly");
                    if (nodeDA == null)
                    {
                        nodeDA = doc.CreateNode(XmlNodeType.Element, "dependentAssembly", null);
                    }

                    XmlElement elemId = doc.CreateElement("assemblyIdentity");
                    elemId.SetAttribute("name", "System.Data.SqlServerCe");
                    elemId.SetAttribute("publicKeyToken", "89845dcd8080cc91");
                    elemId.SetAttribute("culture", "neutral");

                    XmlElement elemBR = doc.CreateElement("bindingRedirect");
                    elemBR.SetAttribute("oldVersion", "3.5.1.0-3.5.1.50");
                    elemBR.SetAttribute("newVersion", "3.5.1.50");

                    nodeDA.AppendChild(elemId);
                    nodeDA.AppendChild(elemBR);

                    nodeAB.AppendChild(nodeDA);

                    nodeRuntime.AppendChild(nodeAB);

                    if (!addRtNode)
                    {
                        XmlNode nodeRunTime = doc.SelectSingleNode("//runtime");
                        if (nodeRunTime != null)
                            nodeRunTime.AppendChild(nodeAB);
                    }
                    else if (!addABNode)
                    {
                        XmlNode nodeAb = doc.SelectSingleNode("//assemblyBinding");
                        if (nodeAb != null)
                            nodeAb.AppendChild(nodeDA);
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
