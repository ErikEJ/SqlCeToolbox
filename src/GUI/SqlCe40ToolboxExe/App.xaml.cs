using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Reflection;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeScripting;
using System.IO;

namespace SqlCeToolboxExe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (ssender, args) =>
            {
                //string[] names = this.GetType().Assembly.GetManifestResourceNames();

                String resourceName = "ErikEJ.SqlCeToolbox." +
                   new AssemblyName(args.Name).Name + ".dll";

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        Byte[] assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                    return null;
                }

            };
            // A file was dropped on the .exe
            if (e.Args.Count() > 0)
            {
                DataConnectionHelper.Argument = e.Args[0];
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (DataConnectionHelper.Monitor != null)
                DataConnectionHelper.Monitor.Stop();
        }
    }
}
