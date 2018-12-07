using System;
using System.Text;
using ErikEJ.SqlCeScripting;

namespace ExportSqlCE
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 6)
            {
                PrintUsageGuide();
                return 2;
            }
            else
            {
                try
                {
                    string connectionString = args[0];
                    string outputFileLocation = args[1];

                    bool includeData = true;
                    bool includeDataForServer = false;
                    bool includeSchema = true;
                    bool saveImageFiles = false;
                    bool sqlAzure = false;
                    bool sqlite = false;
                    bool toExcludeTables = true;
                    bool toIncludeTables = false;
                    System.Collections.Generic.List<string> exclusions = new System.Collections.Generic.List<string>();
                    System.Collections.Generic.List<string> inclusions = new System.Collections.Generic.List<string>();
                    System.Collections.Generic.List<string> whereClauses = new System.Collections.Generic.List<string>();

                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                            
                    if (args[0].Equals("diff", StringComparison.OrdinalIgnoreCase))
                    {
#if V31
                        PrintUsageGuide();
                        return 2;                        
#else
                        if (args.Length == 4)
                        {
                            using (var source = Helper.CreateRepository(args[1]))
                            {
                                using (var target = Helper.CreateRepository(args[2]))
                                {
                                    var generator = Helper.CreateGenerator(source);
                                    SqlCeDiff.CreateDiffScript(source, target, generator, false);
                                    System.IO.File.WriteAllText(args[3], generator.GeneratedScript);
                                    return 0;
                                }
                            }
                        }
                        else
                        {
                            PrintUsageGuide();
                            return 2;
                        }
#endif
                    }
                    else if (args[0].Equals("dgml", StringComparison.OrdinalIgnoreCase))
                    {
#if V31
                        PrintUsageGuide();
                        return 2;
#endif
                        if (args.Length == 3)
                        {
                            using (var source = Helper.CreateRepository(args[1]))
                            {
                                var generator = Helper.CreateGenerator(source, args[2]);
                                generator.GenerateSchemaGraph(args[1]);
                            }
                            return 0;
                        }
                        else
                        {
                            PrintUsageGuide();
                            return 2;
                        }
                    }
                    else if (args[0].Equals("wpdc", StringComparison.OrdinalIgnoreCase))
                    {
#if V31
                        PrintUsageGuide();
                        return 2;
#endif
#if V40
                        PrintUsageGuide();
                        return 2;
#else
                        if (args.Length == 3)
                        {
                            using (var repo = Helper.CreateRepository(args[1]))
                            {
                                var dch = new DataContextHelper();
                                dch.GenerateWPDataContext(repo, args[1], args[2]);
                            }
                            return 0;
                        }
                        else
                        {
                            PrintUsageGuide();
                            return 2;
                        }
#endif
                    }

                    else
                    {
                        for (int i = 2; i < args.Length; i++)
                        {
                            if (args[i].Contains("dataonly"))
                            {
                                includeData = true;
                                includeSchema = false;
                            }
                            if (args[i].Contains("dataonlyserver"))
                            {
                                includeData = true;
                                includeDataForServer = true;
                                includeSchema = false;
                            }
                            if (args[i].Contains("schemaonly"))
                            {
                                includeData = false;
                                includeSchema = true;
                            }
                            if (args[i].Contains("saveimages"))
                                saveImageFiles = true;
                            if (args[i].Contains("sqlazure"))
                                sqlAzure = true;
                            if (args[i].Contains("sqlite"))
                                sqlite = true;
                            if (args[i].StartsWith("exclude:"))
                            {
                                ParseExclusions(exclusions, args[i], whereClauses);
                                toExcludeTables = true;
                                toIncludeTables = false;
                            }
                            if (args[i].StartsWith("include:"))
                            {
                                ParseInclusions(inclusions, args[i], whereClauses);
                                toIncludeTables = true;
                                toExcludeTables = false;
                            }
                        }

                        using (IRepository repository = Helper.CreateRepository(connectionString))
                        {
                            Console.WriteLine("Initializing....");
                            Helper.FinalFiles = outputFileLocation;
#if V40
                            var generator = new Generator4(repository, outputFileLocation, sqlAzure, false, sqlite);
#else
                            var generator = new Generator(repository, outputFileLocation, sqlAzure, false, sqlite);
#endif
                            if (toExcludeTables)
                            {
                                generator.ExcludeTables(exclusions);
                            }
                            else if (toIncludeTables)
                            {
                                generator.IncludeTables(inclusions, whereClauses);
                            }
                            Console.WriteLine("Generating the tables....");
                            if (sqlite)
                            {
                                generator.GenerateSqlitePrefix();
                            }

                            if (includeSchema)
                            {
#if V31
                                generator.GenerateTable(false);
#else
                                generator.GenerateTable(includeData);
#endif
                            }
                            if (sqlite)
                            {
                                Console.WriteLine("Generating the data....");
                                generator.GenerateTableContent(false);
                                Console.WriteLine("Generating the indexes....");
                                generator.GenerateIndex();
                                generator.GenerateSqliteSuffix();
                            }
                            else
                            {
                                if (sqlAzure && includeSchema)
                                {
                                    Console.WriteLine("Generating the primary keys (SQL Azure)....");
                                    generator.GeneratePrimaryKeys();
                                }
                                if (includeData)
                                {
                                    Console.WriteLine("Generating the data....");
                                    generator.GenerateTableContent(saveImageFiles);
                                    if (!includeSchema) // ie. DataOnly
                                    {
                                        Console.WriteLine("Generating IDENTITY reset statements....");
                                        generator.GenerateIdentityResets(includeDataForServer);
                                    }
                                }
                                if (!sqlAzure && includeSchema)
                                {
                                    Console.WriteLine("Generating the primary keys....");
                                    generator.GeneratePrimaryKeys();
                                }
                                if (includeSchema)
                                {
                                    Console.WriteLine("Generating the indexes....");
                                    generator.GenerateIndex();
                                    Console.WriteLine("Generating the foreign keys....");
                                    generator.GenerateForeignKeys();
                                }
                            }
                            Helper.WriteIntoFile(generator.GeneratedScript, outputFileLocation, generator.FileCounter, sqlite);
                        }
                        Console.WriteLine("Sent script to output file(s) : {0} in {1} ms", Helper.FinalFiles, (sw.ElapsedMilliseconds).ToString());
                        return 0;
                    }
                }
                catch (System.Data.SqlServerCe.SqlCeException e)
                {
                    Console.WriteLine(Helper.ShowErrors(e));
                    return 1;
                }
                catch (System.Data.SqlClient.SqlException es)
                {
                    Console.WriteLine(Helper.ShowErrors(es)); 
                    return 1;
                }

                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                    return 1;
                }
            }
        }

        private static void ParseExclusions(System.Collections.Generic.List<string> exclusions, string excludeParam, System.Collections.Generic.List<string> whereClauses)
        {
            ParseTableNames(exclusions, "exclude", excludeParam, whereClauses);
        }

        private static void ParseInclusions(System.Collections.Generic.List<string> inclusions, string includeParam, System.Collections.Generic.List<string> whereClauses)
        {
            ParseTableNames(inclusions, "include", includeParam, whereClauses);
        }

        private static void ParseTableNames(System.Collections.Generic.List<string> tableNames, string argumentName, string argumentParam, System.Collections.Generic.List<string> whereClauses)
        {
            argumentParam = argumentParam.Replace($"{argumentName}:", string.Empty);
            argumentParam = argumentParam.Replace($"\"", string.Empty);
            if (!string.IsNullOrEmpty(argumentParam))
            {
                string[] tables = argumentParam.Split(',');
                foreach (var item in tables)
                {
                    var tableParams = item.Split(':');
                    tableNames.Add(tableParams[0]);
                    whereClauses.Add(tableParams.Length > 1 ? tableParams[1] : null);
                }
            }
        }

        private static void PrintUsageGuide()
        {
            var exeName = " " + System.AppDomain.CurrentDomain.FriendlyName + " ";
            Console.WriteLine("Usage : (To script an entire database)");
            Console.WriteLine(exeName + "[SQL CE Connection String] [output file location] [[exclude]]|[[include]] [[schemaonly|dataonly|dataonlyserver]] [[saveimages]] [[sqlazure]]");
            Console.WriteLine(" (exclude, schemaonly|dataonly, saveimages and sqlazure are optional parameters)");
            Console.WriteLine("");
            Console.WriteLine("Examples : ");
            Console.WriteLine(exeName +"\"Data Source=D:\\Northwind.sdf;\" Northwind.sql");
            Console.WriteLine("");
            Console.WriteLine(exeName + "\"Data Source=D:\\Northwind.sdf;\" Northwind.sql exclude:Shippers,Suppliers");
            Console.WriteLine(exeName + "\"Data Source=D:\\Northwind.sdf;\" Northwind.sql include:Shippers,Suppliers");
            Console.WriteLine(exeName + "\"Data Source=D:\\Northwind.sdf;\" Northwind.sql include:\"dbo.Shippers:ID=1 OR ID=2,dbo.Suppliers:Title LIKE 'Company%'\"");
            Console.WriteLine("");
#if V31
#else
            Console.WriteLine("Usage: (To create a schema diff script)");
            Console.WriteLine(exeName + "diff [SQL Compact or SQL Server Connection String (source)] ");
            Console.WriteLine(" [SQL Compact or SQL Server Connection String (target)] [output file location]");
            Console.WriteLine("Example :");
            Console.WriteLine(exeName + "diff \"Data Source=D:\\Northwind.sdf;\" \"Data Source=.\\SQLEXPRESS,Inital Catalog=Northwind\" NorthwindDiff.sql");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Usage: (To create a database graph)");
            Console.WriteLine(exeName + "dgml [SQL Compact or SQL Server Connection String (source)] [output file location]");
            Console.WriteLine("Example :");
            Console.WriteLine(exeName + "dgml \"Data Source=D:\\Northwind.sdf;\" C:\\temp\\northwind.dgml");
            Console.WriteLine("");
            Console.WriteLine("");
#if V40
#else
            Console.WriteLine("Usage: (To create a Windows Phone DataContext)");
            Console.WriteLine(exeName + "wpdc [SQL Compact or SQL Server Connection String (source)] [output file location]");
            Console.WriteLine("Example :");
            Console.WriteLine(exeName + "wpdc \"Data Source=D:\\Northwind.sdf;\" C:\\temp\\Northwind.cs");
            Console.WriteLine("");
            Console.WriteLine("");
#endif
            Console.WriteLine("Usage : (To script an entire database to SQLite format)");
            Console.WriteLine(exeName + "[SQL CE Connection String] [output file location] [sqlite]");
            Console.WriteLine("");
            Console.WriteLine("Examples : ");
            Console.WriteLine(exeName + "\"Data Source=D:\\Northwind.sdf;\" Northwind.sql sqlite");
            Console.WriteLine("");
#endif
        }
    }
}
