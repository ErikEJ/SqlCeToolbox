/**
 * Copyright (C) 2008, Microsoft Corp.  All Rights Reserved
 */

using System;
using System.Collections.Generic;
using System.Data.Entity.Design;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace EdmGen2
{
    /// <summary>
    /// 
    /// This is a command-line program to perform some common Entity Data 
    /// Model tooling functions on EDMX files.  It is similiar in functionality 
    /// to the .net framework's EdmGen.exe, but it will operate on the ".edmx" 
    /// file format, instead of the .csdl, .ssdl & .msl file formats used by the 
    /// .net framework's EDM.
    /// 
    /// </summary>
    public class EdmGen2
    {
        // a class that understands what the different XML namespaces are for the different EF versions. 
        private static NamespaceManager _namespaceManager = new NamespaceManager();

        // ErikEJ: Have removed some unneeded code, and left the rest
		//sample usage:
			//EdmGen2.EdmGen2.ModelGen("Data Source=C:\\tmp\\northwind.sdf", "System.Data.SqlServerCe.4.0", "Northwind", "C:\\tmp\\", true, true, new List<string>(), new Version(3,0,0,0));
			//foreach (var error in EdmGen2.EdmGen2.Errors)
			//{
			//	Console.WriteLine(error);
			//}

        // Begin Added ErikEJ

        private static string edmxPath;
        public static List<string> Errors = new List<string>();

		public static void ModelGen(
            string connectionString, string provider, string modelName, string edmxFilePath, bool foreignKeys, bool pluralize, List<string> tables, Version version)
        {
            edmxPath = edmxFilePath;
            Errors.Clear();
            ModelGen(connectionString, provider, modelName, version, foreignKeys, pluralize, tables);
        }

        // End Added ErikEJ

        #region the functions that actually do the interesting things

        private static void ModelGen(
            string connectionString, string provider, string modelName, Version version, bool includeForeignKeys, bool pluralize, List<string> tables)
        {
            IList<EdmSchemaError> ssdlErrors = null;
            IList<EdmSchemaError> csdlAndMslErrors = null;

            // generate the SSDL
            string ssdlNamespace = modelName + "Model.Store";
            EntityStoreSchemaGenerator essg =
                new EntityStoreSchemaGenerator(
                    provider, connectionString, ssdlNamespace);
            essg.GenerateForeignKeyProperties = includeForeignKeys;

            //ErikEJ Filter selected tables
            var filters = new List<EntityStoreSchemaFilterEntry>();
            foreach (var table in tables)
            {
                var entry = new EntityStoreSchemaFilterEntry(null, null, table);
                filters.Add(entry);
            }
            ssdlErrors = essg.GenerateStoreMetadata(filters, version);

            // detect if there are errors or only warnings from ssdl generation
            bool hasSsdlErrors = false;
            bool hasSsdlWarnings = false;
            if (ssdlErrors != null)
            {
                foreach (EdmSchemaError e in ssdlErrors)
                {
                    if (e.Severity == EdmSchemaErrorSeverity.Error)
                    {
                        hasSsdlErrors = true;
                    }
                    else if (e.Severity == EdmSchemaErrorSeverity.Warning)
                    {
                        hasSsdlWarnings = true;
                    }
                }
            }

            // write out errors & warnings
            if (hasSsdlErrors && hasSsdlWarnings)
            {
                //System.Console.WriteLine("Errors occurred during generation:");
                WriteErrors(ssdlErrors);
            }

            // if there were errors abort.  Continue if there were only warnings
            if (hasSsdlErrors)
            {
                return;
            }

            // write the SSDL to a string
            using (StringWriter ssdl = new StringWriter())
            {
                XmlWriter ssdlxw = XmlWriter.Create(ssdl);
                essg.WriteStoreSchema(ssdlxw);
                ssdlxw.Flush();

                // generate the CSDL
                string csdlNamespace = modelName + "Model";
                string csdlEntityContainerName = modelName + "Entities";
                EntityModelSchemaGenerator emsg =
                    new EntityModelSchemaGenerator(
                        essg.EntityContainer, csdlNamespace, csdlEntityContainerName);
                emsg.GenerateForeignKeyProperties = includeForeignKeys;

                // Begin Added ErikEJ
                if (pluralize)
                    emsg.PluralizationService = System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(new System.Globalization.CultureInfo("en-US"));
                // End Added ErikEJ
                csdlAndMslErrors = emsg.GenerateMetadata(version);


                // detect if there are errors or only warnings from csdl/msl generation
                bool hasCsdlErrors = false;
                bool hasCsdlWarnings = false;
                if (csdlAndMslErrors != null)
                {
                    foreach (EdmSchemaError e in csdlAndMslErrors)
                    {
                        if (e.Severity == EdmSchemaErrorSeverity.Error)
                        {
                            hasCsdlErrors = true;
                        }
                        else if (e.Severity == EdmSchemaErrorSeverity.Warning)
                        {
                            hasCsdlWarnings = true;
                        }
                    }
                }

                // write out errors & warnings
                if (hasCsdlErrors || hasCsdlWarnings)
                {
                    //System.Console.WriteLine("Errors occurred during generation:");
                    WriteErrors(csdlAndMslErrors);
                }

                // if there were errors, abort.  Don't abort if there were only warnigns.  
                if (hasCsdlErrors)
                {
                    return;
                }

                // write CSDL to a string
                using (StringWriter csdl = new StringWriter())
                {
                    XmlWriter csdlxw = XmlWriter.Create(csdl);
                    emsg.WriteModelSchema(csdlxw);
                    csdlxw.Flush();

                    // write MSL to a string
                    using (StringWriter msl = new StringWriter())
                    {
                        XmlWriter mslxw = XmlWriter.Create(msl);
                        emsg.WriteStorageMapping(mslxw);
                        mslxw.Flush();

                        // write csdl, ssdl & msl to the EDMX file
                        ToEdmx(
                            csdl.ToString(), ssdl.ToString(), msl.ToString(), new FileInfo(
                                modelName + ".edmx"), includeForeignKeys, pluralize);
                    }
                }
            }
        }
        
        private static void ValidateAndGenerateViews(FileInfo edmxFile, LanguageOption languageOption, bool generateViews)
        {
            XDocument doc = XDocument.Load(edmxFile.FullName);
            XElement c = GetCsdlFromEdmx(doc);
            XElement s = GetSsdlFromEdmx(doc);
            XElement m = GetMslFromEdmx(doc);

            // load the csdl
            XmlReader[] cReaders = { c.CreateReader() };
            IList<EdmSchemaError> cErrors = null;
            EdmItemCollection edmItemCollection = 
                MetadataItemCollectionFactory.CreateEdmItemCollection(cReaders, out cErrors);

            // load the ssdl 
            XmlReader[] sReaders = { s.CreateReader() };
            IList<EdmSchemaError> sErrors = null;
            StoreItemCollection storeItemCollection = 
                MetadataItemCollectionFactory.CreateStoreItemCollection(sReaders, out sErrors);

            // load the msl
            XmlReader[] mReaders = { m.CreateReader() };
            IList<EdmSchemaError> mErrors = null;
            StorageMappingItemCollection mappingItemCollection = 
                MetadataItemCollectionFactory.CreateStorageMappingItemCollection(
                edmItemCollection, storeItemCollection, mReaders, out mErrors);

            // either pre-compile views or validate the mappings
            IList<EdmSchemaError> viewGenerationErrors = null;
            if (generateViews)
            {
                // generate views & write them out to a file
                string outputFile =
                    GetFileNameWithNewExtension(edmxFile, ".GeneratedViews" +
                        GetFileExtensionForLanguageOption(languageOption));
                EntityViewGenerator evg = new EntityViewGenerator(languageOption);
                viewGenerationErrors =
                    evg.GenerateViews(mappingItemCollection, outputFile);
            }
            else
            {
                viewGenerationErrors = EntityViewGenerator.Validate(mappingItemCollection);
            }

            // write errors
            WriteErrors(cErrors);
            WriteErrors(sErrors);
            WriteErrors(mErrors);
            WriteErrors(viewGenerationErrors);

        }

        private static void ToEdmx(String c, String s, String m, FileInfo edmxFile, bool includeForeignKeys, bool pluralize)
        {
            // This will strip out any of the xml header info from the xml strings passed in 
            XDocument cDoc = XDocument.Load(new StringReader(c));
            c = cDoc.Root.ToString();
            XDocument sDoc = XDocument.Load(new StringReader(s));
            s = sDoc.Root.ToString();
            XDocument mDoc = XDocument.Load(new StringReader(m));
            // re-write the MSL so it will load in the EDM designer
            FixUpMslForEDMDesigner(mDoc.Root);
            m = mDoc.Root.ToString();

            // get the version to use - we use the root CSDL as the version. 
            Version v = _namespaceManager.GetVersionFromCSDLDocument(cDoc);
            XNamespace edmxNamespace = _namespaceManager.GetEDMXNamespaceForVersion(v);

            // the "Version" attribute in the Edmx element
            string edmxVersion = v.Major + "." + v.MajorRevision;

            StringBuilder sb = new StringBuilder();
            sb.Append("<edmx:Edmx Version=\"" + edmxVersion + "\"");
            sb.Append(" xmlns:edmx=\"" +  edmxNamespace.NamespaceName +"\">");
            sb.Append(Environment.NewLine);
	        sb.Append("<!-- EF Runtime content -->");
			sb.Append(Environment.NewLine);	        
			sb.Append("<edmx:Runtime>");
            sb.Append(Environment.NewLine);
	        sb.Append("<!-- SSDL content -->");
			sb.Append(Environment.NewLine);
            sb.Append("<edmx:StorageModels>");
            sb.Append(Environment.NewLine);
            sb.Append(s);
            sb.Append(Environment.NewLine);
            sb.Append("</edmx:StorageModels>");
            sb.Append(Environment.NewLine);
	        sb.Append("<!-- CSDL content -->");
			sb.Append(Environment.NewLine);
            sb.Append("<edmx:ConceptualModels>");
            sb.Append(Environment.NewLine);
            sb.Append(c);
            sb.Append(Environment.NewLine);
            sb.Append("</edmx:ConceptualModels>");
            sb.Append(Environment.NewLine);
			sb.Append("<!-- C-S mapping content -->");
			sb.Append(Environment.NewLine);
            sb.Append("<edmx:Mappings>");
            sb.Append(Environment.NewLine);
            sb.Append(m);
            sb.Append(Environment.NewLine);
            sb.Append("</edmx:Mappings>");
            sb.Append(Environment.NewLine);
            sb.Append("</edmx:Runtime>");
            sb.Append(Environment.NewLine);
	        sb.Append("<!-- EF Designer content (DO NOT EDIT MANUALLY BELOW HERE) -->");
			sb.Append(Environment.NewLine);
            sb.Append("<edmx:Designer xmlns=\"" + edmxNamespace.NamespaceName +"\">");
            sb.Append(Environment.NewLine);
            sb.Append("<Connection><DesignerInfoPropertySet><DesignerProperty Name=\"MetadataArtifactProcessing\" Value=\"EmbedInOutputAssembly\" /></DesignerInfoPropertySet></Connection>");
            sb.Append(Environment.NewLine);
            sb.Append("<edmx:Options>");
            sb.Append("<DesignerInfoPropertySet>");
            sb.Append("<DesignerProperty Name=\"ValidateOnBuild\" Value=\"True\" />");
            sb.Append("<DesignerProperty Name=\"EnablePluralization\" Value=\"" + pluralize.ToString() + "\" />");
            sb.Append("<DesignerProperty Name=\"IncludeForeignKeysInModel\" Value=\"" + includeForeignKeys.ToString() + "\" />");
	        if (v == new Version(3,0,0,0))
	        {
				sb.Append("<DesignerProperty Name=\"UseLegacyProvider\" Value=\"False\" />");		
				sb.Append("<DesignerProperty Name=\"CodeGenerationStrategy\" Value=\"None\" />");
	        }
	        else
	        {
				sb.Append("<DesignerProperty Name=\"CodeGenerationStrategy\" Value=\"Default\" />");    
	        }	        
            sb.Append("</DesignerInfoPropertySet>");
            sb.Append("</edmx:Options>");
            sb.Append(Environment.NewLine);
            sb.Append("<edmx:Diagrams />");
            sb.Append(Environment.NewLine);
            sb.Append("</edmx:Designer>");
            sb.Append("</edmx:Edmx>");
            sb.Append(Environment.NewLine);

            // Changed ErikEJ
            File.WriteAllText(Path.Combine(edmxPath, edmxFile.Name), sb.ToString());
            // File.WriteAllText(edmxFile.FullName, sb.ToString());
        }
        #endregion

        #region Code to extract the csdl, ssdl & msl sections from an EDMX file

        private static XElement GetCsdlFromEdmx(XDocument xdoc)
        {
            Version version = _namespaceManager.GetVersionFromEDMXDocument(xdoc);
            string csdlNamespace = _namespaceManager.GetCSDLNamespaceForVersion(version).NamespaceName;
            return (from item in xdoc.Descendants(
                        XName.Get("Schema", csdlNamespace)) select item).First();
        }

        private static XElement GetSsdlFromEdmx(XDocument xdoc)
        {
            Version version = _namespaceManager.GetVersionFromEDMXDocument(xdoc);
            string ssdlNamespace = _namespaceManager.GetSSDLNamespaceForVersion(version).NamespaceName;
            return (from item in xdoc.Descendants(
                        XName.Get("Schema", ssdlNamespace)) select item).First();
        }

        private static XElement GetMslFromEdmx(XDocument xdoc)
        {
            Version version = _namespaceManager.GetVersionFromEDMXDocument(xdoc);
            string mslNamespace = _namespaceManager.GetMSLNamespaceForVersion(version).NamespaceName;
            return (from item in xdoc.Descendants(
                        XName.Get("Mapping", mslNamespace)) select item).First();
        }

        #endregion

        #region Some utility functions we use in the program

        private static string GetFileNameWithNewExtension(
            FileInfo file, string extension)
        {
            string prefix = file.Name.Substring(
                0, file.Name.Length - file.Extension.Length);
            return prefix + extension;
        }

        private static void WriteErrors(IEnumerable<EdmSchemaError> errors)
        {
            if (errors != null)
            {
                foreach (EdmSchemaError e in errors)
                {
                    WriteError(e);
                }
            }
        }

        private static void WriteError(EdmSchemaError e)
        {
            string error = string.Empty;
            if (e.Severity == EdmSchemaErrorSeverity.Error)
            {
                //Console.Write("Error:  ");
                error += "Error:  ";
            }
            else
            {
                //Console.Write("Warning:  ");
                error += "Warning:  ";
            }
            error += e.Message;
            Errors.Add(error);
            //Console.WriteLine(e.Message);
        }

        private static string GetFileExtensionForLanguageOption(
            LanguageOption langOption)
        {
            if (langOption == LanguageOption.GenerateCSharpCode)
            {
                return ".cs";
            }
            else
            {
                return ".vb";
            }
        }

        #endregion

        #region "fix-up" code to fix up MSL so that it will load in the EDMX designer

        //
        // This will re-write MSL to remove some syntax that the EDM Designer 
        // doesn't support.  Specifically, the designer doesn't support 
        //     - the "TypeName" attribute in "EntitySetMapping" elements
        //     - the "StoreEntitySet" attribute in "EntityTypeMapping" and 
        //       "EntitySetMapping" elements.   
        //
        private static void FixUpMslForEDMDesigner(XElement mappingRoot)
        {

            XName n1 = XName.Get("EntityContainerMapping", mappingRoot.Name.NamespaceName);
            XName n2 = XName.Get("EntitySetMapping", mappingRoot.Name.NamespaceName);
            XName n3 = XName.Get("EntityTypeMapping", mappingRoot.Name.NamespaceName);

            foreach (XElement e1 in mappingRoot.Elements(n1))
            {
                // process EntitySetMapping nodes
                foreach (XElement e2 in e1.Elements(n2))
                {
                    XAttribute typeNameAttribute = null;
                    XAttribute storeEntitySetAttribute = null;

                    foreach (XAttribute a in e2.Attributes())
                    {
                        if (a.Name.Equals(XName.Get("TypeName", "")))
                        {
                            typeNameAttribute = a;
                            break;
                        }
                    }

                    if (typeNameAttribute != null)
                    {
                        FixUpEntitySetMapping(typeNameAttribute, e2);
                    }

                    // process EntityTypeMappings
                    foreach (XElement e3 in e2.Elements(n3))
                    {
                        foreach (XAttribute a in e3.Attributes())
                        {
                            if (a.Name.Equals(XName.Get("StoreEntitySet", "")))
                            {
                                storeEntitySetAttribute = a;
                                break;
                            }
                        }

                        if (storeEntitySetAttribute != null)
                        {
                            FixUpEntityTypeMapping(storeEntitySetAttribute, e3);
                        }
                    }
                }
            }
        }

        private static void FixUpEntitySetMapping(
            XAttribute typeNameAttribute, XElement entitySetMappingNode)
        {
            XName xn = XName.Get("EntityTypeMapping", entitySetMappingNode.Name.NamespaceName);

            typeNameAttribute.Remove();
            XElement etm = new XElement(xn);
            etm.Add(typeNameAttribute);

            // move the "storeEntitySet" attribute into the new 
            // EntityTypeMapping node
            foreach (XAttribute a in entitySetMappingNode.Attributes())
            {
                if (a.Name.LocalName == "StoreEntitySet")
                {
                    a.Remove();
                    etm.Add(a);
                    break;
                }
            }

            // now move all descendants into this node
            ReparentChildren(entitySetMappingNode, etm);

            entitySetMappingNode.Add(etm);
        }

        private static void FixUpEntityTypeMapping(
            XAttribute storeEntitySetAttribute, XElement entityTypeMappingNode)
        {
            XName xn = XName.Get("MappingFragment", entityTypeMappingNode.Name.NamespaceName);
            XElement mf = new XElement(xn);

            // move the StoreEntitySet attribute into this node
            storeEntitySetAttribute.Remove();
            mf.Add(storeEntitySetAttribute);

            // now move all descendants into this node
            ReparentChildren(entityTypeMappingNode, mf);

            entityTypeMappingNode.Add(mf);
        }

        private static void ReparentChildren(
            XContainer originalParent, XContainer newParent)
        {
            // re-parent all descendants from originalParent into newParent
            List<XNode> childNodes = new List<XNode>();
            foreach (XNode d in originalParent.Nodes())
            {
                childNodes.Add(d);
            }
            foreach (XNode d in childNodes)
            {
                d.Remove();
                newParent.Add(d);
            }
        }
        #endregion

        public static string GetEDMXDiagram()
        {
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?><edmx:Edmx Version=\"3.0\" xmlns:edmx=\"http://schemas.microsoft.com/ado/2009/11/edmx\"><!-- EF Designer content (DO NOT EDIT MANUALLY BELOW HERE) --><edmx:Designer xmlns=\"http://schemas.microsoft.com/ado/2009/11/edmx\"><!-- Diagram content (shape and connector positions) --><edmx:Diagrams></edmx:Diagrams></edmx:Designer></edmx:Edmx>";
        }
    }


    #region NameSpaceManager
    internal class NamespaceManager
    {
        private static Version v1 = EntityFrameworkVersions.Version1;
        private static Version v2 = EntityFrameworkVersions.Version2;
		private static Version v3 = new Version(3,0,0,0);

        private Dictionary<Version, XNamespace> _versionToCSDLNamespace = new Dictionary<Version, XNamespace>() 
        { 
        { v1, XNamespace.Get("http://schemas.microsoft.com/ado/2006/04/edm") }, 
        { v2, XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/edm") },
		{ v3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm") },
        };

        private Dictionary<Version, XNamespace> _versionToSSDLNamespace = new Dictionary<Version, XNamespace>() 
        { 
        { v1, XNamespace.Get("http://schemas.microsoft.com/ado/2006/04/edm/ssdl") }, 
        { v2, XNamespace.Get("http://schemas.microsoft.com/ado/2009/02/edm/ssdl") },
		{ v3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm/ssdl") }
        };

        private Dictionary<Version, XNamespace> _versionToMSLNamespace = new Dictionary<Version, XNamespace>() 
        { 
        { v1, XNamespace.Get("urn:schemas-microsoft-com:windows:storage:mapping:CS") }, 
        { v2, XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/mapping/cs") }, 
		{ v3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/mapping/cs") } 
        };


        private Dictionary<Version, XNamespace> _versionToEDMXNamespace = new Dictionary<Version, XNamespace>() 
        { 
        { v1, XNamespace.Get("http://schemas.microsoft.com/ado/2007/06/edmx") }, 
        { v2, XNamespace.Get("http://schemas.microsoft.com/ado/2008/10/edmx") }, 
        { v3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edmx") } 
        };

        private Dictionary<XNamespace, Version> _namespaceToVersion = new Dictionary<XNamespace, Version>();

        internal NamespaceManager()
        {
            foreach (KeyValuePair<Version, XNamespace> kvp in _versionToCSDLNamespace)
            {
                _namespaceToVersion.Add(kvp.Value, kvp.Key);
            }

            foreach (KeyValuePair<Version, XNamespace> kvp in _versionToSSDLNamespace)
            {
                _namespaceToVersion.Add(kvp.Value, kvp.Key);
            }

            foreach (KeyValuePair<Version, XNamespace> kvp in _versionToMSLNamespace)
            {
                _namespaceToVersion.Add(kvp.Value, kvp.Key);
            }

            foreach (KeyValuePair<Version, XNamespace> kvp in _versionToEDMXNamespace)
            {
                _namespaceToVersion.Add(kvp.Value, kvp.Key);
            }
        }

        internal Version GetVersionFromEDMXDocument(XDocument xdoc)
        {
            XElement el = xdoc.Root;
            if (el.Name.LocalName.Equals("Edmx") == false)
            {
                throw new ArgumentException("Unexpected root node local name for edmx document");
            }
            return this.GetVersionForNamespace(el.Name.Namespace);
        }

        internal Version GetVersionFromCSDLDocument(XDocument xdoc)
        {
            XElement el = xdoc.Root;
            if (el.Name.LocalName.Equals("Schema") == false)
            {
                throw new ArgumentException("Unexpected root node local name for csdl document");
            }
            return this.GetVersionForNamespace(el.Name.Namespace);
        }

        internal XNamespace GetMSLNamespaceForVersion(Version v)
        {
            XNamespace n;
            _versionToMSLNamespace.TryGetValue(v, out n);
            return n;
        }

        internal XNamespace GetCSDLNamespaceForVersion(Version v)
        {
            XNamespace n;
            _versionToCSDLNamespace.TryGetValue(v, out n);
            return n;
        }

        internal XNamespace GetSSDLNamespaceForVersion(Version v)
        {
            XNamespace n;
            _versionToSSDLNamespace.TryGetValue(v, out n);
            return n;
        }

        internal XNamespace GetEDMXNamespaceForVersion(Version v)
        {
            XNamespace n;
            _versionToEDMXNamespace.TryGetValue(v, out n);
            return n;
        }

        internal Version GetVersionForNamespace(XNamespace n)
        {
            Version v;
            _namespaceToVersion.TryGetValue(n, out v);
            return v;
        }
    }
    #endregion

}
