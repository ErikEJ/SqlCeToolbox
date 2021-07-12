using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using NuGet.VisualStudio;
using System;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    public class NuGetHelper
    {
        public void InstallPackage(string packageId, Project project)
        {
            var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
            var nuGetInstaller = componentModel.GetService<IVsPackageInstaller>();
            nuGetInstaller?.InstallPackage(null, project, packageId, (Version)null, false);
        }
    }
}
