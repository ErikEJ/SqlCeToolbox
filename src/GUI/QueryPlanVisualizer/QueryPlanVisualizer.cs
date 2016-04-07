using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ExecutionPlanVisualizer.Properties;

namespace ExecutionPlanVisualizer
{
    public static class QueryPlanVisualizer
    {
        private static bool _shouldExtract = true;

        public static string BuildPlanHtmml(string planXml)
        {
            if (string.IsNullOrEmpty(planXml))
            {
                //ShowError("Cannot retrieve query plan");
                return string.Empty;   
            }

            var queryPlanProcessor = new QueryPlanProcessor(planXml);

            var planHtml = queryPlanProcessor.ConvertPlanToHtml();

            var files = ExtractFiles();
            files.Add(planHtml);

            // ReSharper disable once CoVariantArrayConversion
            return string.Format(Resources.template, files.ToArray());
        }

        private static List<string> ExtractFiles()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QueryVisualizer");
            var imagesFolder = Path.Combine(folder, "images");

            if (!Directory.Exists(folder))
            {
                _shouldExtract = true;
                Directory.CreateDirectory(folder);
            }

            if (!Directory.Exists(imagesFolder))
            {
                _shouldExtract = true;
                Directory.CreateDirectory(imagesFolder);
            }

            var qpJavascript = Path.Combine(folder, "qp.js");
            var qpStyleSheet = Path.Combine(folder, "qp.css");
            var jquery = Path.Combine(folder, "jquery.js");

            if (_shouldExtract)
            {
                File.WriteAllText(qpJavascript, Resources.jquery);
                File.WriteAllText(qpStyleSheet, Resources.qpStyleSheet);
                File.WriteAllText(jquery, Resources.qpJavascript);

                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();

                foreach (var name in resourceNames.Where(name => name.EndsWith(".gif")))
                {
                    using (var stream = assembly.GetManifestResourceStream(name))
                    {
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
                        if (fileNameWithoutExtension != null)
                            using (var file = new FileStream(Path.Combine(imagesFolder, fileNameWithoutExtension.Split('.').Last() + ".gif"), FileMode.Create, FileAccess.Write))
                            {
                                if (stream != null) stream.CopyTo(file);
                            }
                    }
                }

                _shouldExtract = false;
            }

            return new List<string> { qpStyleSheet, qpJavascript, jquery };
        }
    }
}