using EnvDTE;
using System.IO;
using System.Reflection;
using System;

namespace EFCorePowerTools.Extensions
{
    internal static class ProjectExtensions
    {
        public const int S_OK = 0;

        public static bool TryBuild(this Project project)
        {
            var dte = project.DTE;
            var configuration = dte.Solution.SolutionBuild.ActiveConfiguration.Name;

            dte.Solution.SolutionBuild.BuildProject(configuration, project.UniqueName, true);

            return dte.Solution.SolutionBuild.LastBuildInfo == 0;
        }

        public static string GetOutPutAssemblyPath(this Project project)
        {
            var assemblyName = project.Properties.Item("AssemblyName").Value.ToString();

            if (assemblyName == null) return null;

            var assemblyNameExe = assemblyName + ".exe";
            var assemblyNameDll = assemblyName + ".dll";

            var outputPath = GetOutputPath(project);

            if (File.Exists(Path.Combine(outputPath, assemblyNameExe)))
            {
                return Path.Combine(outputPath, assemblyNameExe);
            }

            if (File.Exists(Path.Combine(outputPath, assemblyNameDll)))
            {
                return Path.Combine(outputPath, assemblyNameDll);
            }

            return null;
        }

        private static string GetOutputPath(Project project)
        {
            string absoluteOutputPath = null;

            var configManager = project.ConfigurationManager;
            if (configManager == null) return null;

            var activeConfig = configManager.ActiveConfiguration;
            var outputPath = activeConfig.Properties.Item("OutputPath").Value.ToString();
            var fullName = project.FullName;

            absoluteOutputPath = ReverseEngineer20.PathHelper.GetAbsPath(outputPath, fullName);

            return absoluteOutputPath;
        }
 
    }
}