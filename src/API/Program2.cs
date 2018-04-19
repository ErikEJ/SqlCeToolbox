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
                    bool includeSchema = true;
                    bool saveImageFiles = false;
                    bool keepSchemaName = false;
                    bool preserveDateAndDateTime2 = false;
                    bool sqlite = false;
                    System.Collections.Generic.List<string> exclusions = new System.Collections.Generic.List<string>();

                    for (int i = 2; i < args.Length; i++)
                    {
                        if (args[i].StartsWith("dataonly"))
                        {
                            includeData = true;
                            includeSchema = false;
                        }
                        if (args[i].StartsWith("schemaonly"))
                        {
                            includeData = false;
                            includeSchema = true;
                        }
                        if (args[i].StartsWith("saveimages"))
                            saveImageFiles = true;
                        if (args[i].StartsWith("keepschema"))
                            keepSchemaName = true;
                        if (args[i].StartsWith("preservedateanddatetime2"))
                            preserveDateAndDateTime2 = true;
                        if (args[i].StartsWith("exclude:"))
                            ParseExclusions(exclusions, args[i]);
                        if (args[i].StartsWith("sqlite"))
                        {
                            sqlite = true;
                            includeSchema = true;
                        }
                    }

                    using (IRepository repository = new ServerDBRepository(connectionString, keepSchemaName))
                    {
                        Helper.FinalFiles = outputFileLocation;
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        var generator = new Generator(repository, outputFileLocation, false, preserveDateAndDateTime2, sqlite);

                        generator.ExcludeTables(exclusions);
                        if (sqlite)
                        {
                            generator.GenerateSqlitePrefix();
                            if (includeSchema)
                            {
                                Console.WriteLine("Generating the tables....");
                                generator.GenerateTable(false);
                            }
                            if (includeData)
                            {
                                Console.WriteLine("Generating the data....");
                                generator.GenerateTableContent(false);
                            }
                            if (includeSchema)
                            {
                                Console.WriteLine("Generating the indexes....");
                                generator.GenerateIndex();
                            }
                            generator.GenerateSqliteSuffix();
                        }
                        else
                        {
                            // The execution below has to be in this sequence
                            if (includeSchema)
                            {
                                Console.WriteLine("Generating the tables....");
                                generator.GenerateTable(includeData);
                            }
                            if (includeData)
                            {
                                Console.WriteLine("Generating the data....");
                                generator.GenerateTableContent(saveImageFiles);
                            }
                            if (includeSchema)
                            {
                                Console.WriteLine("Generating the primary keys....");
                                generator.GeneratePrimaryKeys();
                                Console.WriteLine("Generating the indexes....");
                                generator.GenerateIndex();
                                Console.WriteLine("Generating the foreign keys....");
                                generator.GenerateForeignKeys();
                            }
                        }
                        Helper.WriteIntoFile(generator.GeneratedScript, outputFileLocation, generator.FileCounter, sqlite);
                        Console.WriteLine("Sent script to output file(s) : {0} in {1} ms", Helper.FinalFiles, (sw.ElapsedMilliseconds).ToString());
                        return 0;
                    }
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    ShowErrors(e);
                    return 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                    return 1;
                }
            }
        }

        private static void ParseExclusions(System.Collections.Generic.List<string> exclusions, string excludeParam)
        {
            excludeParam = excludeParam.Replace("exclude:", string.Empty);
            if (!string.IsNullOrEmpty(excludeParam))
            {
                string[] tables = excludeParam.Split(',');
                foreach (var item in tables)
                {
                    exclusions.Add(item);
                }
            }
        }

        private static void ShowErrors(System.Data.SqlClient.SqlException e)
        {
            System.Data.SqlClient.SqlErrorCollection errorCollection = e.Errors;

            StringBuilder bld = new StringBuilder();
            Exception inner = e.InnerException;

            if (null != inner)
            {
                Console.WriteLine("Inner Exception: " + inner.ToString());
            }
            // Enumerate the errors to a message box.
            foreach (System.Data.SqlClient.SqlError err in errorCollection)
            {
                bld.Append("\n Message   : " + err.Message);
                bld.Append("\n Source    : " + err.Source);
                bld.Append("\n Number    : " + err.Number);

                Console.WriteLine(bld.ToString());
                bld.Remove(0, bld.Length);
            }
        }

        private static void PrintUsageGuide()
        {
            Console.WriteLine("Usage : ");
            Console.WriteLine(" Export2SQLCE.exe [SQL Server Connection String] [output file location] [[exclude]] [[schemaonly]] [[dataonly]] [[saveimages]] [[sqlite]] [[preservedateanddatetime2]] [[keepschema]]");
            Console.WriteLine(" (exclude, schemaonly, dataonly, saveimages, sqlite, keepschema and preservedateanddatetime2 are optional parameters)");
            Console.WriteLine("");
            Console.WriteLine("Examples : ");
            Console.WriteLine(" Export2SQLCE.exe \"Data Source=(local);Initial Catalog=Northwind;Integrated Security=True\" Northwind.sql");
            Console.WriteLine(" Export2SQLCE.exe \"Data Source=(local);Initial Catalog=Northwind;Integrated Security=True\" Northwind.sql schemaonly");
            Console.WriteLine(" Export2SQLCE.exe \"Data Source=(local);Initial Catalog=Northwind;Integrated Security=True\" Northwind.sql dataonly");
            Console.WriteLine(" Export2SQLCE.exe \"Data Source=(local);Initial Catalog=Northwind;Integrated Security=True\" Northwind.sql exclude:dbo.Shippers,dbo.Suppliers");
            Console.WriteLine(" Export2SQLCE.exe \"Data Source=(local);Initial Catalog=Northwind;Integrated Security=True\" Northwind.sql sqlite");
            Console.WriteLine("");
        }
    }
}
