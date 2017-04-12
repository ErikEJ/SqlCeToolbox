using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;

namespace ErikEJ.SqlCeToolbox.SSMSEngine
{
    public class ObjectExplorerManager
    {
        private readonly SqlCeToolboxPackage _package;
        private HierarchyObject _serverMenu;
        private string _urnPath = "Server/Database";

        public ObjectExplorerManager(SqlCeToolboxPackage package)
        {
            _package = package;
        }

        public Dictionary<string, DatabaseInfo> GetAllServerUserDatabases(string ssmsVersion)
        {
            var result = new Dictionary<string, DatabaseInfo>();
            try
            {
                if (ssmsVersion == "13")
                {
                    var servers = new List<Microsoft.SqlServer.Management.Common.SqlConnectionInfo>();

                    foreach (var srvHerarchy in GetExplorerHierarchies())
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        var provider = srvHerarchy.Root as IServiceProvider;

                        if (provider == null) continue;
                        var containedItem = provider.GetService(typeof(INodeInformation)) as INodeInformation;
                        if (containedItem != null) servers.Add(containedItem.Connection as Microsoft.SqlServer.Management.Common.SqlConnectionInfo);
                    }

                    foreach (var sqlConnectionInfo in servers)
                    {
                        var builder = new SqlConnectionStringBuilder(sqlConnectionInfo.ConnectionString);
                        AddToList(result, builder);
                    }
                }
                if (ssmsVersion == "14")
                {
                    //TODO Use v140 ref!
                }
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex);
            }
            return result;
        }

        private void AddToList(Dictionary<string, DatabaseInfo> result, SqlConnectionStringBuilder builder)
        {
            var databaseNames = GetDatabaseNames(builder);
            foreach (var databaseName in databaseNames)
            {
                builder.InitialCatalog = databaseName.Item2;
                var databaseInfo = new DatabaseInfo
                {
                    Caption = databaseName.Item1 + "." + databaseName.Item2,
                    ConnectionString = builder.ConnectionString,
                    DatabaseType = DatabaseType.SQLServer,
                    FromServerExplorer = true
                };
                result.Add(builder.ConnectionString, databaseInfo);
            }
        }

        private List<Tuple<string, string>> GetDatabaseNames(SqlConnectionStringBuilder builder)
        {
            var sql = @"SELECT @@servername AS ServerName, name AS DatabaseName FROM sys.databases
                WHERE state = 0 AND name NOT IN ('master', 'model', 'tempdb', 'msdb', 'Resource');";
            var result = new List<Tuple<string, string>>();
            builder.InitialCatalog = "master";
            using (var conn = new SqlConnection(builder.ConnectionString))
            {
                using (var command = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    command.CommandTimeout = 5;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(new Tuple<string, string>(
                            reader[0].ToString(), reader[1].ToString()));
                    }
                }
            }
            return result;
        }

        private IObjectExplorerService GetObjectExplorer()
        {
            return _package.GetServiceHelper(typeof(IObjectExplorerService)) as IObjectExplorerService;
        }

        public void SetObjectExplorerEventProvider()
        {
            var mi = GetType().GetMethod("Provider_SelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            var objectExplorer = GetObjectExplorer();
            var t = Assembly.Load("Microsoft.SqlServer.Management.SqlStudio.Explorer").GetType("Microsoft.SqlServer.Management.SqlStudio.Explorer.ObjectExplorerService");

            int nodeCount;
            INodeInformation[] nodes;
            objectExplorer.GetSelectedNodes(out nodeCount, out nodes);

            var piContainer = t.GetProperty("Container", BindingFlags.Public | BindingFlags.Instance);
            var objectExplorerContainer = piContainer.GetValue(objectExplorer, null);
            var piContextService = objectExplorerContainer.GetType().GetProperty("Components", BindingFlags.Public | BindingFlags.Instance);
            //object[] indexArgs = { 1 };
            var objectExplorerComponents = piContextService.GetValue(objectExplorerContainer, null) as ComponentCollection;
            object contextService = null;

            if (objectExplorerComponents != null)
                foreach (Component component in objectExplorerComponents)
                {
                    if (component.GetType().FullName.Contains("ContextService"))
                    {
                        contextService = component;
                        break;
                    }
                }
            if (contextService == null)
                throw new NullReferenceException("Can't find ObjectExplorer ContextService.");

            var piObjectExplorerContext = contextService.GetType().GetProperty("ObjectExplorerContext", BindingFlags.Public | BindingFlags.Instance);
            var objectExplorerContext = piObjectExplorerContext.GetValue(contextService, null);
            var ei = objectExplorerContext.GetType().GetEvent("CurrentContextChanged", BindingFlags.Public | BindingFlags.Instance);
            if (ei == null) return;
            var del = Delegate.CreateDelegate(ei.EventHandlerType, this, mi);
            ei.AddEventHandler(objectExplorerContext, del);
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void Provider_SelectionChanged(object sender, NodesChangedEventArgs args)
        {
            if (args.ChangedNodes.Count <= 0) return;
            var node = args.ChangedNodes[0];
            if (node == null) return;
            Debug.WriteLine(node.UrnPath);
            Debug.WriteLine(node.Name);
            Debug.WriteLine(node.Context);
            if (_serverMenu == null && _urnPath == node.UrnPath)
            {
                _serverMenu = (HierarchyObject)node.GetService(typeof(IMenuHandler));
                //var separator = new ToolStripSeparatorMenuItem();
                //_serverMenu.AddChild(string.Empty, separator);
                var item = new DatabaseMenuItem(_package);
                _serverMenu.AddChild(string.Empty, item);
            }
        }

        private object GetTreeControl()
        {
            Type t = GetObjectExplorer().GetType();
            PropertyInfo treeProperty = t.GetProperty("Tree", BindingFlags.Instance | BindingFlags.NonPublic);
            var objectTreeControl = treeProperty.GetValue(GetObjectExplorer(), null);
            return objectTreeControl;
        }

        private IEnumerable<IExplorerHierarchy> GetExplorerHierarchies()
        {
            var objectTreeControl = GetTreeControl();
            var objTreeRype = objectTreeControl.GetType();
            var hierFieldInfo = objTreeRype.GetField("hierarchies", BindingFlags.Instance | BindingFlags.NonPublic);
            if (hierFieldInfo != null)
            {
                var hierDictionary = (IEnumerable<KeyValuePair<string, IExplorerHierarchy>>) hierFieldInfo.GetValue(objectTreeControl);

                foreach (var keyVaklue in hierDictionary)
                {
                    yield return keyVaklue.Value;
                }
            }
        }
    }
}

