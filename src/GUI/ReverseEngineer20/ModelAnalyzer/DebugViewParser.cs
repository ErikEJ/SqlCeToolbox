using System;
using System.Collections.Generic;
using System.Linq;

//Model: 
//  EntityType: Alert
//    Properties: 
//      AlertId(long) Required PK AfterSave:Throw ValueGenerated.OnAdd 0 0 0 -1 0
//        Annotations: 

//         Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.LongTypeMapping
//     Acknowledged (bool) Required 1 1 -1 -1 -1

//       Annotations: 

//         Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.BoolTypeMapping
//     Active (bool) Required 2 2 -1 -1 -1

//       Annotations: 

//         Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.BoolTypeMapping
//     AlertText (string) 3 3 -1 -1 -1

//       Annotations: 

//         Relational:ColumnType: nvarchar(4000)
//          Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerStringTypeMapping
//      AlertTime(DateTime) Required 4 4 -1 -1 -1
//        Annotations: 
//          Relational:ColumnType: datetime
//          Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerDateTimeTypeMapping
//      Category(int) Required ValueGenerated.OnAdd 5 5 -1 -1 1
//        Annotations: 

//         Relational:DefaultValueSql: ((0))
//          Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.IntTypeMapping
//      Details(string) 6 6 -1 -1 -1
//        Annotations: 
//          Relational:ColumnType: nvarchar(4000)
//          Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerStringTypeMapping
//      Reported(bool) Required 7 7 -1 -1 -1
//        Annotations: 
//          Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.BoolTypeMapping
//      Resolution(int) Required ValueGenerated.OnAdd 8 8 -1 -1 2
//        Annotations: 
//         Relational:DefaultValueSql: ((0))
//          Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.IntTypeMapping
//      SessionId(long) Required 9 9 -1 -1 -1
//        Annotations: 
//          Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.LongTypeMapping
//      Severity(int) Required 10 10 -1 -1 -1
//        Annotations: 
//          Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.IntTypeMapping
//      Status(int) Required ValueGenerated.OnAdd 11 11 -1 -1 3
//        Annotations: 

//         Relational:DefaultValueSql: ((0))
//          Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.IntTypeMapping
//      SystemErrorGuid(Guid) Required 12 12 -1 -1 -1
//        Annotations: 
//          Relational:TypeMapping: Microsoft.EntityFrameworkCore.Storage.GuidTypeMapping
//    Keys: 
//      AlertId PK
//    Annotations: 
//      Relational:TableName: Alert
//      RelationshipDiscoveryConvention:NavigationCandidates: System.Collections.Immutable.ImmutableSortedDictionary`2[System.Reflection.PropertyInfo,System.Type]
//Annotations: 
//ProductVersion: 2.0.0-rtm-26452
//SqlServer:ValueGenerationStrategy: IdentityColumn


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
                        productVersion = line.Split(' ')[1];
                    }
                    if (!line.TrimStart().StartsWith("ProductVersion: " ) &&
                        !line.TrimStart().StartsWith("Annotations:"))
                    {
                        modelAnnotations += line.Trim() + " ";
                    }
                }
            }
            result.Nodes.Add(
                $"<Node Id=\"Model\" Label=\"{dbContextName}\" ProductVersion=\"{productVersion}\" ProviderAnnotations=\"{modelAnnotations}\" Category=\"Model\" Group=\"Expanded\" />");

   //EntityType: PackageCode
   //           Properties: 
   //   PackageCodeId(long) Required PK AfterSave: Throw ValueGenerated.OnAdd 0 0 0 - 1 0
   //     Annotations:
   //         Relational: TypeMapping: Microsoft.EntityFrameworkCore.Storage.LongTypeMapping
   //   NdcId(long) Required 1 1 - 1 - 1 - 1
   //     Annotations:
   //         Relational: TypeMapping: Microsoft.EntityFrameworkCore.Storage.LongTypeMapping
   //   PackageNdc(string) 2 2 - 1 - 1 - 1
   //     Annotations:
   //         Relational: ColumnType: nvarchar(4000)
   //       Relational: TypeMapping: Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerStringTypeMapping

            var entityName = string.Empty;
            var properties = new List<string>();
            var propertyLinks = new List<string>();
            var inProperties = false;
            var inOtherProperties = false;
            foreach (var line in debugViewLines)
            {
                if (line.TrimStart().StartsWith("EntityType:"))
                {
                    if (!string.IsNullOrEmpty(entityName))
                    {
                        result.Nodes.Add(
                            $"<Node Id = \"{entityName}\" Label=\"{entityName}\" Category=\"EntityType\" Group=\"Collapsed\" />");
                        result.Links.Add(
                            $"<Link Source = \"Model\" Target=\"{entityName}\" Category=\"Contains\" />");
                        result.Nodes.AddRange(properties);
                        result.Links.AddRange(propertyLinks);
                        properties.Clear();
                        propertyLinks.Clear();
                    }
                    entityName = line.Trim().Split(' ')[1];
                    inProperties = false;
                }
                if (line.TrimStart().StartsWith("Properties:"))
                {
                    inProperties = true;
                    inOtherProperties = false;
                }

                if (!string.IsNullOrEmpty(entityName) && inProperties)
                {
                    //TODO Improve!
                    if (line.StartsWith("    Keys:")
                    ||  line.StartsWith("    Navigations:")
                    ||  line.StartsWith("    Annotations:"))
                    {
                        inOtherProperties = true;
                        continue;
                    }
                    //TODO Use regex here!
                    if (line.StartsWith("      ") && !inOtherProperties)
                    {
                        if (line.StartsWith("        Annotations:")
                         || line.StartsWith("          Relational:"))
                        {
                            continue;
                        }
                        //0       1      2+        
                        //AlertId (long) Required PK AfterSave:Throw ValueGenerated.OnAdd 
                        //TODO What is AfterSave:Throw ?
                        //TODO Backing field!

                        var props = line.Trim().Split(' ').ToList();
                        var name = props[0];
                        var type = System.Security.SecurityElement.Escape(props[1]);
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
                            $"<Node Id = \"{entityName}_{name}\" Label=\"{name}\" Category=\"{category}\" Type=\"{type}\" IsPrimaryKey=\"{isPrimaryKey}\" IsForeignKey=\"{isForeignKey}\" IsRequired=\"{isRequired}\" IsIndexed=\"{isIndexed}\" ValueGenerated=\"{valueGenerated}\" />");

                        propertyLinks.Add($"<Link Source = \"{entityName}\" Target=\"{entityName}_{name}\" Category=\"Contains\" />");

                        //<Property Id = "Type" Label="Type" Description="CLR data type" Group="Model Properties" DataType="System.String" />
                        //<Property Id = "Field" Label ="Field" Description="Backing field" Group="Model Properties" DataType="System.String" />
                        //<Property Id = "IsIndexed" Group="Model Flags" DataType="System.Boolean" />
                        //<Property Id = "IsRequired" Group="Model Flags" DataType="System.Boolean" />
                        //<Property Id = "IsPrimaryKey" Group="Model Flags" DataType="System.Boolean" />
                        //<Property Id = "IsForeignKey" Group="Model Flags" DataType="System.Boolean" />
                        //<Property Id = "Valuegeneration"  Group="Model Flags" DataType="System.String" />

                        //<Property Id = "ColumnType" Label="Column Type" Description="Relational data type" Group="Model Properties" DataType="System.String" />
                        //<Property Id = "ProviderAnnotations" Label="Provider Annotations" Description="Provider specific annotations" Group="Model Properties" DataType="System.String" />
                        //<Property Id = "TableName" Label="Table name" Description="EF Core product version" Group="Model Properties" DataType="System.String" />
                    }
                }
            }

            return result;
        }
    }
}



//<Property Id = "Field" Label ="Field" Description="Backing field" Group="Model Properties" DataType="System.String" />
//<Property Id = "Type" Label="Type" Description="CLR data type" Group="Model Properties" DataType="System.String" />
//<Property Id = "ColumnType" Label="Column Type" Description="Relational data type" Group="Model Properties" DataType="System.String" />
//<Property Id = "ProductVersion" Label="Product Version" Description="EF Core product version" Group="Model Properties" DataType="System.String" />
//<Property Id = "ProviderAnnotations" Label="Provider Annotations" Description="Provider specific annotations" Group="Model Properties" DataType="System.String" />
//<Property Id = "TableName" Label="Table name" Description="EF Core product version" Group="Model Properties" DataType="System.String" />
//<Property Id = "IsIndexed" Group="Model Flags" DataType="System.Boolean" />
//<Property Id = "IsRequired" Group="Model Flags" DataType="System.Boolean" />
//<Property Id = "IsPrimaryKey" Group="Model Flags" DataType="System.Boolean" />
//<Property Id = "IsForeignKey" Group="Model Flags" DataType="System.Boolean" />
//<Property Id = "IsValueGenerated" Group="Model Flags" DataType="System.Boolean" /> 
//<Property Id = "Valuegeneration"  Group="Model Flags" DataType="System.String" />

//<Node Id = "Database" Label="NorthwindEF7.sdf" Category="Database" Group="Expanded" />
//<Node Id = "Categories" Label="Categories" Category="Table" Group="Collapsed" />
//<Node Id = "Categories_CategoryID" Label="CategoryID" Category="Field Primary" Description="int" />
//<Node Id = "Categories_CategoryName" Label="CategoryName" Category="Field" Description="nvarchar(15)" />
//<Node Id = "Categories_Description" Label="Description" Category="Field Optional" Description="ntext" />

//<Link Source = "Database" Target="Categories" Category="Contains" />
//<Link Source = "Categories" Target="Categories_CategoryID" Category="Contains" />
//<Link Source = "Categories" Target="Categories_CategoryName" Category="Contains" />
//<Link Source = "Categories" Target="Categories_Description" Category="Contains" />
//<Link Source = "Categories" Target="Categories_Picture" Category="Contains" />
