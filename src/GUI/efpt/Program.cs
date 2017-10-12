using System;
using System.Collections.Generic;

namespace ReverseEngineer20
{
    public class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    var builder = new EfCoreModelBuilder();
                    List<Tuple<string, string>> result;

                    if (args[0].ToLowerInvariant() == "ddl" && args.Length == 2)
                    {
                        result = builder.GenerateDatabaseCreateScript(args[1]);
                    }
                    else
                    {
                        result = builder.GenerateDebugView(args[0]);
                    }
                    foreach (var tuple in result)
                    {
                        Console.Out.WriteLine("DbContext:");
                        Console.Out.WriteLine(tuple.Item1);
                        Console.Out.WriteLine("DebugView:");
                        Console.Out.WriteLine(tuple.Item2);
                    }
                }
                else
                {
                    Console.Out.WriteLine("Error:");
                    Console.Out.WriteLine("Invalid command line");
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Error:");
                Console.Out.WriteLine(ex);
                return 1;
            }
        }
    }
}
