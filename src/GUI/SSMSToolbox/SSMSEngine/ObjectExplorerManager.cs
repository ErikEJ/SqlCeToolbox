using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;

namespace ErikEJ.SqlCeToolbox
{
    public class ObjectExplorerManager
    {
        private readonly SqlCeToolboxPackage _package;
        private IObjectExplorerService _objExplorer;

        public ObjectExplorerManager(SqlCeToolboxPackage package)
        {
            _package = package;
        }

        public List<SqlConnectionInfo> GetAllServers()
        {
            try
            {
                List<SqlConnectionInfo> servers = new List<SqlConnectionInfo>();

                foreach (IExplorerHierarchy srvHerarchy in GetExplorerHierarchies())
                {
                    var provider = srvHerarchy.Root as IServiceProvider;

                    if (provider == null) continue;
                    var containedItem = provider.GetService(typeof(INodeInformation)) as INodeInformation;
                    if (containedItem != null) servers.Add(containedItem.Connection as SqlConnectionInfo);
                }

                return servers;
            }
            catch (Exception ex)
            {
                ////TODO Log to Activity log
                //log.Error("ObjectExplorer manager failed:" + ex.Message, ex);
                throw ex;
            }
        }

        public IObjectExplorerService GetObjectExplorer()
        {
            return _package.GetServiceHelper(typeof(IObjectExplorerService)) as IObjectExplorerService;
        }

        private Object GetTreeControl()
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

