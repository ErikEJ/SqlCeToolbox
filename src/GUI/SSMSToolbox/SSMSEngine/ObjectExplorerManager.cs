using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;

namespace ErikEJ.SqlCeToolbox
{
    public class ObjectExplorerManager
    {

        private SqlCeToolboxPackage _package;

        public ObjectExplorerManager(SqlCeToolboxPackage package)
        {
            _package = package;
        }

        private static IObjectExplorerService _objExplorer = null;

        public List<SqlConnectionInfo> GetAllServers()
        {
            try
            {
                List<SqlConnectionInfo> servers = new List<SqlConnectionInfo>();

                foreach (IExplorerHierarchy srvHerarchy in GetExplorerHierarchies())
                {
                    IServiceProvider provider = srvHerarchy.Root as IServiceProvider;

                    if (provider != null)
                    {
                        INodeInformation containedItem = provider.GetService(typeof(INodeInformation)) as INodeInformation;
                        if (containedItem != null) servers.Add(containedItem.Connection as SqlConnectionInfo);
                    }
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

        private IObjectExplorerService GetObjectExplorer()
        {
            if (_objExplorer == null)
            {
                _objExplorer = _package.GetServiceHelper(typeof(IObjectExplorerService)) as IObjectExplorerService;
            }
            return _objExplorer;
        }

        private Object GetTreeControl()
        {
            Type t = GetObjectExplorer().GetType();
            PropertyInfo treeProperty = t.GetProperty("Tree", BindingFlags.Instance | BindingFlags.NonPublic);
            var objectTreeControl = treeProperty.GetValue(GetObjectExplorer(), null);
            return objectTreeControl;
        }

        // ugly reflection hack
        private IExplorerHierarchy GetHierarchyForConnection(SqlConnectionInfo connection)
        {
            IExplorerHierarchy hierarchy;
            var objectTreeControl = GetTreeControl();
            var objTreeRype = objectTreeControl.GetType();
            var getHierarchyMethod = objTreeRype.GetMethod("GetHierarchy", BindingFlags.Instance | BindingFlags.Public);
            hierarchy = getHierarchyMethod.Invoke(objectTreeControl, new Object[] { connection, String.Empty }) as IExplorerHierarchy;

            // VS2008 here we have additional param String.Empty - need Dependency Injection in order to make it work?
            return hierarchy;
        }

        private IEnumerable<IExplorerHierarchy> GetExplorerHierarchies()
        {
            var objectTreeControl = GetTreeControl();
            var objTreeRype = objectTreeControl.GetType();
            var hierFieldInfo = objTreeRype.GetField("hierarchies", BindingFlags.Instance | BindingFlags.NonPublic);
            var hierDictionary = (IEnumerable<KeyValuePair<string, IExplorerHierarchy>>) hierFieldInfo.GetValue(objectTreeControl);

            foreach (var keyVaklue in hierDictionary)
            {
                yield return keyVaklue.Value;
            }
        }

        

        // select server on object window
        public void SelectServer(SqlConnectionInfo connection)
        {
            IExplorerHierarchy hierarchy = GetHierarchyForConnection(connection);
            SelectNode(hierarchy.Root);
        }        

        public static String CreateFile(String script)
        {
            var path = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", new Object[] { Path.GetTempFileName(), "dtq" });
            var builder = new StringBuilder();
            builder.Append("[D035CF15-9EDB-4855-AF42-88E6F6E66540, 2.00]\r\n");
            builder.Append("Begin Query = \"Query1.dtq\"\r\n");
            builder.Append("Begin RunProperties =\r\n");
            builder.AppendFormat("{0}{1}{2}", "SQL = \"", script, "\"\r\n");
            builder.Append("ParamPrefix = \"@\"\r\n");
            builder.Append("ParamSuffix = \"\"\r\n");
            builder.Append("ParamSuffix = \"\\\"\r\n");
            builder.Append("End\r\n");
            builder.Append("End\r\n");

            using (var writer = new StreamWriter(path, false, Encoding.Unicode))
            {
                writer.Write(builder.ToString());
            }

            return path;
        }

        internal void OpenTable(NamedSmoObject objectToSelect, SqlConnectionInfo connection)
        {
            try
            {
                IExplorerHierarchy hierarchy = GetHierarchyForConnection(connection);

                if (hierarchy == null)
                {
                    return; // there is nothing we can really do if we don't have one of these
                }

                HierarchyTreeNode databasesNode = GetUserDatabasesNode(hierarchy.Root);
                var resultNode = FindNodeForSmoObject(databasesNode, objectToSelect);

                //MSSQLController.Current.SearchWindow.Activate();

                if (resultNode != null)
                {
                    OpenTable(resultNode, connection);
                }
            }
            catch (Exception ex)
            {
                //TODO Log to Activity log
                //log.Error("Error opening table: " + objectToSelect.Name, ex);
            }
        }

        internal void SelectSMOObjectInObjectExplorer(NamedSmoObject objectToSelect, SqlConnectionInfo connection)
        {
            if (objectToSelect.State == SqlSmoState.Dropped)
            {
                //TODO Log to Activity log
                //log.Info("Trying to locate dropped object:" + objectToSelect.Name);
                return;
            }

            IExplorerHierarchy hierarchy = GetHierarchyForConnection(connection);

            if (hierarchy == null)
            {
                return; // there is nothing we can really do if we don't have one of these
            }

            HierarchyTreeNode databasesNode = GetUserDatabasesNode(hierarchy.Root);
            var resultNode = FindNodeForSmoObject(databasesNode, objectToSelect);

            if (resultNode != null)
            {
                SelectNode(resultNode);
            }
        }

        private HierarchyTreeNode GetUserDatabasesNode(HierarchyTreeNode rootNode)
        {
            if (rootNode != null)
            {
                // root should always be expandable
                if (rootNode.Expandable)
                {
                    EnumerateChildrenSynchronously(rootNode);
                    rootNode.Expand();

                    // TODO this is horrible code - it assumes the first node will ALWAYS be the "Databases" node in the object explorer, which may not always be the case
                    // however I couldn't think of a clean way to always find the right node
                    return (HierarchyTreeNode) rootNode.Nodes[0];
                }
            }

            return null;
        }

        private string GetNodeNameFor(NamedSmoObject smoObject)
        {
            return smoObject.ToString().Replace("[", "").Replace("]", "");
        }

        private HierarchyTreeNode FindTableNode(HierarchyTreeNode nodeDatabases, NamedSmoObject tableSmoObject)
        {
            var tableToSelect = (Table)tableSmoObject;
            return FindRecursively(nodeDatabases, tableToSelect.Parent, "Tables", GetNodeNameFor(tableSmoObject));
        }

        private HierarchyTreeNode FindNodeForSmoObject(HierarchyTreeNode nodeDatabases, NamedSmoObject objectToSelect)
        {
            if (objectToSelect is Table)
            {
                return FindTableNode(nodeDatabases, objectToSelect);
            }
            else if(objectToSelect is View)
            {
                var viewToSelect = (View)objectToSelect;
                return FindRecursively(nodeDatabases, viewToSelect.Parent, "Views", GetNodeNameFor(objectToSelect));
            }
            else if (objectToSelect is StoredProcedure)
            {
                var procedure = (StoredProcedure)objectToSelect;
                return FindRecursively(nodeDatabases, procedure.Parent, "Programmability", "Stored Procedures", GetNodeNameFor(objectToSelect));
            }
            //else if (objectToSelect is UserDefinedFunction)
            //{
            //    var func = (UserDefinedFunction)objectToSelect;
            //    string functionNodeName = func.FunctionType == UserDefinedFunctionType.Scalar?"Scalar-valued Functions":"Table-valued Functions";
            //    return FindRecursively(nodeDatabases, func.Parent, "Programmability", "Functions", functionNodeName, GetNodeNameFor(objectToSelect));
            //}

            return null;

        }

        private HierarchyTreeNode FindRecursively(HierarchyTreeNode parent,Database database, params string[] nodes)
        {
            var databaseNode = FindDatabaseNodeByName(parent, database);
            if (databaseNode == null)
                return null;

            HierarchyTreeNode currentLevel = databaseNode;
            foreach(var nodeName in nodes)
            {
                 currentLevel = FindChildNodeByName(currentLevel, nodeName);

                 if (currentLevel == null)
                 {
                     return null;
                 }

            }

            return currentLevel;
        }

        private HierarchyTreeNode FindDatabaseNodeByName(HierarchyTreeNode parentNode, Database database)
        {
            var databaseNode =  FindChildNodeByName(parentNode, database.Name);
            if (databaseNode != null)
                return databaseNode;

            // trying to find node with (Read-Only) text at the end as read only database node will change its text
            var readonlyDatabaseName = FindChildNodeByName(parentNode, database.Name + " (Read-Only)");
            if (readonlyDatabaseName != null)
                return readonlyDatabaseName;

            var standbyDatabaseName = FindChildNodeByName(parentNode, database.Name + " (Standby / Read-Only)");
            return standbyDatabaseName;
        }

        private HierarchyTreeNode FindChildNodeByName(HierarchyTreeNode parentNode, string name)
        {
             if (!parentNode.Expandable)
                 return null;
                           
            EnumerateChildrenSynchronously(parentNode);
            parentNode.Expand();

            foreach (HierarchyTreeNode child in parentNode.Nodes)
            {
                if (child.Text.ToLower() == name.ToLower())
                    return child;
            }

            return null;             
        } 

    
        private void OpenTable(HierarchyTreeNode node, SqlConnectionInfo connection)
        {
            var t = Type.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer.OpenTableHelperClass,ObjectExplorer", true, true);
            var mi = t.GetMethod("EditTopNRows", BindingFlags.Static | BindingFlags.Public);
            var ncT = Type.GetType("Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer.NodeContext,ObjectExplorer", true, true);

            IServiceProvider provider = node as IServiceProvider;
            INodeInformation containedItem = provider.GetService(typeof(INodeInformation)) as INodeInformation;

            var inst = Activator.CreateInstance(ncT, containedItem);

            if (inst == null)
            {
                throw new Exception("Cannot create type" + ncT.ToString());
            }

            mi.Invoke(null, new Object[] { containedItem, 200 });
        }

        private void SelectNode(HierarchyTreeNode node)
        {
            IServiceProvider provider = node as IServiceProvider;

            if (provider != null)
            {
                INodeInformation containedItem = provider.GetService(typeof(INodeInformation)) as INodeInformation;

                if (containedItem != null)
                {
                    IObjectExplorerService objExplorer = GetObjectExplorer();
                    objExplorer.SynchronizeTree(containedItem);
                }
            }
        }

        // another exciting opportunity to use reflection
        private void EnumerateChildrenSynchronously(HierarchyTreeNode node)
        {
            Type t = node.GetType();
            MethodInfo method = t.GetMethod("EnumerateChildren", new Type[] { typeof(Boolean) });

            if (method != null)
            {
                method.Invoke(node, new Object[] { false });
            }
            else
            {
                // fail
                node.EnumerateChildren();
            }
        }
    }
}
