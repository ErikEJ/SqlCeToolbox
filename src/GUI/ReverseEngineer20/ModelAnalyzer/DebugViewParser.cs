using System;
using System.Collections.Generic;
using System.Linq;

namespace ReverseEngineer20.ModelAnalyzer
{
    public class DebugViewParser
    {
        public DebugViewParserResult Parse(string[] debugViewLines, string dbContextName)
        {
            var result = new DebugViewParserResult();

            var modelAnnotated = false;
            var productVersion = string.Empty;
            var modelAnnotations = string.Empty;

            foreach (var line in debugViewLines)
            {
                if (line.StartsWith("Annotations:"))
                {
                    modelAnnotated = true;
                }
                if (modelAnnotated)
                {
                    if (line.TrimStart().StartsWith("ProductVersion: "))
                    {
                        productVersion = line.Trim().Split(' ')[1];
                    }
                    if (!line.TrimStart().StartsWith("ProductVersion: " ) &&
                        !line.TrimStart().StartsWith("Annotations:"))
                    {
                        modelAnnotations += line.Trim() + Environment.NewLine;
                    }
                }
            }
            result.Nodes.Add(
                $"<Node Id=\"Model\" Label=\"{dbContextName}\" ProductVersion=\"{productVersion}\" Annotations=\"{modelAnnotations.Trim()}\" Category=\"Model\" Group=\"Expanded\" />");

            var entityName = string.Empty;
            var properties = new List<string>();
            var propertyLinks = new List<string>();
            var inProperties = false;
            var inOtherProperties = false;
            var i = -1;
            foreach (var line in debugViewLines)
            {
                i++;
                if (line.TrimStart().StartsWith("EntityType:"))
                {
                    entityName = BuildEntity(debugViewLines, entityName, i, result, properties, propertyLinks, line, ref inProperties);
                }
                if (line.TrimStart().StartsWith("Properties:"))
                {
                    inProperties = true;
                    inOtherProperties = false;
                }

                if (!string.IsNullOrEmpty(entityName) && inProperties)
                {
                    if (line.StartsWith("    Keys:")
                    ||  line.StartsWith("    Navigations:")
                    ||  line.StartsWith("    Annotations:")
                    ||  line.StartsWith("    Foreign keys:"))
                    {
                        inOtherProperties = true;
                        continue;
                    }
                    if (line.StartsWith("      ") && !inOtherProperties)
                    {
                        var annotations = GetAnnotations(i, debugViewLines);

                        //TODO Navigations ?
                        var navigations = GetNavigationNodes(i, debugViewLines);

                        var foreignKeysFragment = GetForeignKeys(i, debugViewLines);

                        if (line.StartsWith("        Annotations:")
                         || line.StartsWith("          "))
                         {
                            continue;
                        }

                        var annotation = string.Join(Environment.NewLine, annotations);
                        
                        var props = line.Trim().Split(' ').ToList();

                        var name = props[0];
                        var field = GetTypeValue(props[1], true);
                        var type = GetTypeValue(props[1], false);

                        props.RemoveRange(0, 2);

                        var isRequired = props.Contains("Required");
                        var isIndexed = props.Contains("Index");
                        var isPrimaryKey = props.Contains("PK");
                        var isForeignKey = props.Contains("FK");
                        var valueGenerated = props.FirstOrDefault(p => p.StartsWith("ValueGenerated."));
                        var category = "Property";
                        if (isForeignKey) category = "Property Foreign";
                        if (isPrimaryKey) category = "Property Primary";

                        properties.Add(
                            $"<Node Id = \"{entityName}.{name}\" Label=\"{name}\" Name=\"{name}\" Category=\"{category}\" Type=\"{type}\" Field=\"{field}\" Annotations=\"{annotation}\" IsPrimaryKey=\"{isPrimaryKey}\" IsForeignKey=\"{isForeignKey}\" IsRequired=\"{isRequired}\" IsIndexed=\"{isIndexed}\" ValueGenerated=\"{valueGenerated}\" />");

                        propertyLinks.Add($"<Link Source = \"{entityName}\" Target=\"{entityName}.{name}\" Category=\"Contains\" />");

                        propertyLinks.AddRange(ParseForeignKeys(foreignKeysFragment));
                    }
                }
            }
            BuildEntity(debugViewLines, entityName, i, result, properties, propertyLinks, null, ref inProperties);
            return result;
        }

        private string BuildEntity(string[] debugViewLines, string entityName, int i, DebugViewParserResult result,
            List<string> properties, List<string> propertyLinks, string line, ref bool inProperties)
        {
            if (!string.IsNullOrEmpty(entityName))
            {
                var annotations = GetEntityAnnotations(i, debugViewLines);
                var annotation = string.Join(Environment.NewLine, annotations);

                result.Nodes.Add(
                    $"<Node Id = \"{entityName}\" Label=\"{entityName}\" Name=\"{entityName}\" Annotations=\"{annotation}\" Category=\"EntityType\" Group=\"Collapsed\" />");
                result.Links.Add(
                    $"<Link Source = \"Model\" Target=\"{entityName}\" Category=\"Contains\" />");
                result.Nodes.AddRange(properties);
                result.Links.AddRange(propertyLinks);
                properties.Clear();
                propertyLinks.Clear();
            }
            if (!string.IsNullOrEmpty(line))
                entityName = line.Trim().Split(' ')[1];
            inProperties = false;
            return entityName;
        }

        private IEnumerable<string> ParseForeignKeys(List<string> foreignKeysFragments)
        {
            var links = new List<string>();
            int i = 0;
            var annotation = new List<string>();
            if (foreignKeysFragments.Count > 1)
            {
                foreach (var foreignKeysFragment in foreignKeysFragments)
                {
                    i++;
                    var trim = foreignKeysFragment.Trim();

                    if (trim == "Foreign keys:") continue;

                    if (trim == "Annotations:")
                    {
                        annotation = GetFkAnnotations(i, foreignKeysFragments.ToArray());
                        continue;
                    }

                    if (trim == "Relational:") continue;
                    
                    //TODO Test with multi key FKs!
                    var parts = trim.Split(' ');

                    var source = parts[0]
                                 + "."
                                 //TODO improve
                                 + parts[1].Replace("{", string.Empty).Replace("}", string.Empty)
                                     .Replace("'", string.Empty);
                    var target = parts[3]
                                 + "."
                                 //TODO improve
                                 + parts[4].Replace("{", string.Empty).Replace("}", string.Empty)
                                     .Replace("'", string.Empty);

                    links.Add($"<Link Source=\"{source}\" Target=\"{target}\" Label=\"{source + " -> " + target}\" Category=\"Foreign Key\" />");
                    annotation.Clear();
                    //OrderNdc {'NdcId'} -> Ndc {'NdcId'} ToDependent: OrderNdc ToPrincipal: Ndc
                }
            }
//       Foreign keys: 
//OrderNdc {'NdcId'} -> Ndc {'NdcId'} ToDependent: OrderNdc ToPrincipal: Ndc
//  Annotations: 
//    Relational:Name: FK_dbo.OrderNdc_dbo.Ndc_NdcId

            return links;
        }

        private string GetTypeValue(string type, bool asField)
        {
            var i = asField ? 0 : 1;
            var result = type.Replace("(", string.Empty).Replace(")", string.Empty);
            if (result.Contains(","))
            {
                return System.Security.SecurityElement.Escape(result.Split(',')[i]);
            }
            return asField ? string.Empty : System.Security.SecurityElement.Escape(result);
        }

        private List<string> GetForeignKeys(int i, string[] debugViewLines)
        {
            var x = i;
            var navigations = new List<string>();
            var maxLength = debugViewLines.Length - 1;
            bool inNavigations = false;
            while (x++ < maxLength)
            {
                var trim = debugViewLines[x].Trim();
                if (!inNavigations) inNavigations = trim == "Foreign keys:";

                if (debugViewLines[x].StartsWith("    Annotations:")
                    || debugViewLines[x].StartsWith("Annotations:")
                    || trim.StartsWith("EntityType:"))
                {
                    break;
                }
                if (inNavigations) navigations.Add(trim);
            }

            return navigations;
        }

        private List<string> GetEntityAnnotations(int i, string[] debugViewLines)
        {
            var x = i;
            var values = new List<string>();
            var maxLength = debugViewLines.Length - 1;
            bool inTheMix = false;
            while (x++ < maxLength)
            {
                var trim = debugViewLines[x].Trim();
                if (!inTheMix) inTheMix = debugViewLines[x] == "    Annotations: ";

                if (debugViewLines[x].StartsWith("Annotations:")
                    || trim.StartsWith("EntityType:"))
                {
                    break;
                }
                if (inTheMix && !trim.StartsWith("Annotations:")) values.Add(trim);
            }

            return values;
        }

        private List<string> GetNavigationNodes(int i, string[] debugViewLines)
        {
            var x = i;
            var navigations = new List<string>();
            var maxLength = debugViewLines.Length - 1;
            while (x++ < maxLength && debugViewLines[x] == "    Navigations: ")
            {
                while (x++ < maxLength)
                {
                    var trim = debugViewLines[x].Trim();
                    if (trim.StartsWith("Keys:")
                        || debugViewLines[x].StartsWith("    Annotations:")
                        || debugViewLines[x].StartsWith("Annotations:")
                        || trim.StartsWith("EntityType:")
                        || trim.StartsWith("Foreign Keys:")
                        || trim.StartsWith("Keys:"))
                    {
                        break;
                    }
                    navigations.Add(debugViewLines[x].Trim());
                }
            }

            return navigations;
        }

        private List<string> GetAnnotations(int i, string[] debugViewLines)
        {
            var x = i;
            var annotations = new List<string>();
            var maxLength = debugViewLines.Length - 1;
            if (x++ < maxLength && debugViewLines[x] == "        Annotations: ")
            {
                while (x++ < maxLength && debugViewLines[x].StartsWith("        "))
                {
                    annotations.Add(debugViewLines[x].Trim());
                }
            }

            return annotations;
        }

        private List<string> GetFkAnnotations(int i, string[] debugViewLines)
        {
            var x = i;
            var annotations = new List<string>();
            var maxLength = debugViewLines.Length - 1;
            while (x++ < maxLength)
            {
                if (debugViewLines[x].StartsWith("    "))
                {
                    annotations.Add(debugViewLines[x].Trim());
                }
                if (debugViewLines[x].StartsWith("    Foreign Keys:"))
                    break;
            }

            return annotations;
        }
    }
}
