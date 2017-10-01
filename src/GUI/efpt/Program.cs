using System;

namespace ReverseEngineer20
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    var builder = new EFCoreModelBuilder();
                    var result = builder.GenerateDebugView(args[0]);
                    foreach (var tuple in result)
                    {
                        Console.Out.WriteLine("DbContext:");
                        Console.Out.WriteLine(tuple.Item1);
                        Console.Out.WriteLine("DebugView:");
                        Console.Out.WriteLine(tuple.Item2);
                    }
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
