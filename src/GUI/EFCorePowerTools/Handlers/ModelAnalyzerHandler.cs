using EnvDTE;
using ErikEJ.SqlCeToolbox.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace EFCorePowerTools.Handlers
{
    internal class ModelAnalyzerHandler
    {
        private readonly EFCorePowerToolsPackage _package;

        private const string ptexe = "efpt.exe";

        public ModelAnalyzerHandler(EFCorePowerToolsPackage package)
        {
            _package = package;
        }

        public void GenerateDgml(string outputPath, Project project)
        {
            var dgmlBuilder = new DgmlBuilder.DgmlBuilder();

            try
            {
                //TODO!!!
                //DropFiles(outputPath);
                var modelInfo = LaunchProcess(outputPath);

                if (modelInfo.StartsWith("Error:"))
                {
                    throw new ArgumentException(modelInfo);
                }

                var result = BuildModelInfo(modelInfo);

                ProjectItem item = null;

                foreach (var info in result)
                {
                    var dgmlText = dgmlBuilder.Build(info.Item1, info.Item2, GetTemplate());

                    var path = Path.GetTempFileName() + ".dgml";
                    File.WriteAllText(path, dgmlText, Encoding.UTF8);
                    item = project.ProjectItems.AddFromFile(path);
                }

                if (item != null)
                {
                    var window = item.Open();
                    window.Document.Activate();
                }
            }
            catch (Exception exception)
            {
                _package.LogError(new List<string>(), exception);
            }
        }

        private void DropFiles(string outputPath)
        {
            var toDir = Path.GetDirectoryName(outputPath);
            var fromDir = Assembly.GetExecutingAssembly().Location;

            File.Copy(Path.Combine(fromDir, ptexe), Path.Combine(toDir, ptexe));
            File.Copy(Path.Combine(fromDir, "efpt.exe.config"), Path.Combine(toDir, "efpt.exe.config"));
            File.Copy(Path.Combine(fromDir, "Microsoft.EntityFrameworkCore.Design.dll"), Path.Combine(toDir, "Microsoft.EntityFrameworkCore.Design.dll"));
        }

        private string LaunchProcess(string outputPath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(Path.GetDirectoryName(outputPath), ptexe),
                Arguments = "\"" + outputPath + "\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var standardOutput = new StringBuilder();
            using (var process = System.Diagnostics.Process.Start(startInfo))
            {
                while (!process.HasExited)
                {
                    standardOutput.Append(process.StandardOutput.ReadToEnd());
                }
                standardOutput.Append(process.StandardOutput.ReadToEnd());
            }
            return standardOutput.ToString();
        }

        private List<Tuple<string, string>> BuildModelInfo(string modelInfo)
        {
            //TODO!!
            return new List<Tuple<string, string>>();
        }

        public async void InstallDgmlNuget(Project project)
        {
            _package.Dte2.StatusBar.Text = "Installing DbContext Dgml extension package";
            var nuGetHelper = new NuGetHelper();
            await nuGetHelper.InstallPackageAsync("ErikEJ.EntityFrameworkCore.DgmlBuilder", project);
            _package.Dte2.StatusBar.Text = "Dgml package installed";
            var path = Path.GetTempFileName() + ".txt";
            File.WriteAllText(path, GetReadme(), Encoding.UTF8);
            var window = _package.Dte2.ItemOperations.OpenFile(path);
            window.Document.Activate();
        }

        private string GetReadme()
        {
            var resourceName = "EFCorePowerTools.DgmlBuilder.readme.txt";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private string GetTemplate()
        {
            var resourceName = "EFCorePowerTools.DgmlBuilder.template.dgml";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}