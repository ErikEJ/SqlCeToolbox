using System;
using System.Collections.Generic;
using EnvDTE;
using Microsoft.Win32;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal class EnvDteHelper
    {
        public Project GetProject(DTE dte)
        {
            foreach (SelectedItem item in dte.SelectedItems)
            {
                if (item.Project != null) return item.Project;
                if (item.ProjectItem != null) return item.ProjectItem.ContainingProject;
            }
            return null;
        }

        public ProjectItem GetProjectConfig(Project project)
        {
            foreach (ProjectItem item in project.ProjectItems)
            {
                if (item.Name.ToLowerInvariant() == "app.config")
                {
                    return item;
                }
            }
            return null;
        }

        public ProjectItem GetProjectEdmx(Project project, string model)
        {
            foreach (ProjectItem item in project.ProjectItems)
            {
                if (item.Name.ToLowerInvariant() == model.ToLowerInvariant() + ".edmx")
                {
                    return item;
                }
            }
            return null;
        }

        public ProjectItem GetProjectDc(Project project, string model, string extension)
        {
            foreach (ProjectItem item in project.ProjectItems)
            {
                if (item.Name.ToLowerInvariant() == model.ToLowerInvariant() + extension)
                {
                    return item;
                }
            }
            return null;
        }

        public ProjectItem GetProjectDataContextClass(Project project, string classFileName)
        {
            foreach (ProjectItem item in project.ProjectItems)
            {
                if (item.Name.ToLowerInvariant() == classFileName)
                {
                    return item;
                }
            }
            return null;
        }

        public static void AddReference(Project project, string reference)
        {
            VSLangProj.VSProject vsProject = (VSLangProj.VSProject)project.Object;
            vsProject.References.Add(reference);
        }

        public bool ContainsEfSqlCeReference(Project project)
        {
            VSLangProj.VSProject vsProject = (VSLangProj.VSProject)project.Object;
            for (int i = 1; i < vsProject.References.Count + 1; i++)
            {
                if (vsProject.References.Item(i).Name == "EntityFramework.SqlServerCompact"
                    && new Version(vsProject.References.Item(i).Version) >= new Version(6, 0, 0, 0))
                    return true;
            }
            return false;
        }

        public HashSet<string> GetSqlCeFilesInActiveSolution(DTE dte)
        {
            var list = new HashSet<string>();

            if (!dte.Solution.IsOpen)
                return list;

            foreach (Project project in dte.Solution.Projects)
            {
                GetSqlCeFilesInProject(project, list);
            }

            return list;
        }

        public string GetInitialFolder(DTE dte)
        {
            if (!dte.Solution.IsOpen)
                return null;
            return System.IO.Path.GetDirectoryName(dte.Solution.FullName);
        }

        private void GetSqlCeFilesInProject(Project project, HashSet<string> list)
        {
            if (project.ProjectItems != null)
                GetSqlCeFilesInProjectItems(project.ProjectItems, list);
 	    }

        private void GetSqlCeFilesInProjectItems(ProjectItems projectItems, HashSet<string> list)
        {
            if (projectItems == null)
                return;

            foreach (ProjectItem item in projectItems)
            {
                if (item.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
                {
                    string path = item.Properties.Item("FullPath").Value.ToString();
                    foreach (var extension in GetFileExtensions())
                    {
                        if (path.EndsWith(extension, true, System.Globalization.CultureInfo.InvariantCulture))
                        {
                            if (!list.Contains(path))
                                list.Add(path);
                        }
                    }
                }
                if (item.SubProject != null)
                {
                    GetSqlCeFilesInProject(item.SubProject, list);
                }
                else
                {
                    GetSqlCeFilesInProjectItems(item.ProjectItems, list);
                }
            }
        }

        private List<string> GetFileExtensions()
        {
            var list = new List<string>();
            var sdfExtensions = Properties.Settings.Default.FileFilterSqlCe.Split(new Char[] { ';' });
            foreach (var ext in sdfExtensions)
            {
                list.Add(ext.Replace("*", string.Empty));
            }
            var sqliteExtensions = Properties.Settings.Default.FileFilterSqlite.Split(new Char[] { ';' });
            foreach (var ext in sqliteExtensions)
            {
                list.Add(ext.Replace("*", string.Empty));
            }
            return list;
        }

        public bool ContainsEfSqlCeLegacyReference(Project project)
        {
            VSLangProj.VSProject vsProject = (VSLangProj.VSProject)project.Object;
            for (int i = 1; i < vsProject.References.Count + 1; i++)
            {
                if (vsProject.References.Item(i).Name == "EntityFramework.SqlServerCompact.Legacy"
                    && new Version(vsProject.References.Item(i).Version) >= new Version(6, 0, 0, 0))
                    return true;
            }
            return false;
        }

        internal bool ContainsEfsqLiteReference(Project project)
        {
            VSLangProj.VSProject vsProject = (VSLangProj.VSProject)project.Object;
            for (int i = 1; i < vsProject.References.Count + 1; i++)
            {
                if (vsProject.References.Item(i).Name == "System.Data.SQLite.EF6"
                    && new Version(vsProject.References.Item(i).Version) >= new Version(1, 0, 93, 0))
                    return true;
            }
            return false;
        }

        public List<Guid> AllowedWpProjectKinds
        {
            get
            {
                return new List<Guid>() 
                {
                    new Guid("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"),
                    new Guid("{F184B08F-C81C-45F6-A57F-5ABD9991F28F}")
                };
            }
        }

        public static Guid VbProject = new Guid("{F184B08F-C81C-45F6-A57F-5ABD9991F28F}");

        public static void LaunchUrl(string url)
        {
            var dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) as DTE;
            if (dte != null)
            {
                dte.ItemOperations.Navigate(url);
            }
        }

        public List<Guid> AllowedProjectKinds
        {
            // http://www.mztools.com/Articles/2008/MZ2008017.aspx

            get
            {
                return new List<Guid>() 
                {
                    //Windows (C#) 
                    new Guid("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"),
                    //Windows (VB.NET)
                    new Guid("{F184B08F-C81C-45F6-A57F-5ABD9991F28F}"),
                    //Web Application 
                    new Guid("{349C5851-65DF-11DA-9384-00065B846F21}"),
                    //Web Site 
                    new Guid("{E24C65DC-7377-472B-9ABA-BC803B73C61A}"),
                    //Windows Communication Foundation (WCF)
                    new Guid("{3D9AD99F-2412-4246-B90B-4EAA41C64699}"),
                    //Windows Presentation Foundation (WPF)
                    new Guid("{60DC8134-EBA5-43B8-BCC9-BB4BC16C2548}"),
                    //Universal App Shared
                    new Guid("{D954291E-2A0B-460D-934E-DC6B0785DB48}")
                };
            }
        }

        public List<Guid> WebProjectKinds
        {
            get
            {
                return new List<Guid>() 
                {
                    new Guid("{349C5851-65DF-11DA-9384-00065B846F21}"),
                    new Guid("{E24C65DC-7377-472B-9ABA-BC803B73C61A}")                    
                };
            }
        }


        /// <summary>
        /// Gets the installation directory of a given Visual Studio Version
        /// </summary>
        /// <param name="version">Visual Studio Version</param>
        /// <returns>Null if not installed the installation directory otherwise</returns>
        internal string GetVisualStudioInstallationDir(Version version)
        {
            string registryKeyString = String.Format(@"SOFTWARE{0}Microsoft\VisualStudio\{1}",
                Environment.Is64BitProcess ? @"\Wow6432Node\" : @"\",
                version.ToString(2));

            using (RegistryKey localMachineKey = Registry.LocalMachine.OpenSubKey(registryKeyString))
            {
                if (localMachineKey != null) return localMachineKey.GetValue("InstallDir") as string;
            }
            return null;
        }

        // <summary>
        //     Helper method to show an error message within the shell.  This should be used
        //     instead of MessageBox.Show();
        // </summary>
        // <param name="errorText">Text to display.</param>
        public static void ShowError(string errorText)
        {
            ShowMessageBox(
                errorText, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_CRITICAL);
        }

        public static DialogResult ShowMessage(string messageText)
        {
            return ShowMessageBox(messageText, null, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_INFO);
        }

        // <summary>
        //     Helper method to show a message box within the shell.
        // </summary>
        // <param name="messageText">Text to show.</param>
        // <param name="messageButtons">Buttons which should appear in the dialog.</param>
        // <param name="defaultButton">Default button (invoked when user presses return).</param>
        // <param name="messageIcon">Icon (warning, error, informational, etc.) to display</param>
        // <returns>result corresponding to the button clicked by the user.</returns>
        public static DialogResult ShowMessageBox(
            string messageText, OLEMSGBUTTON messageButtons, OLEMSGDEFBUTTON defaultButton,
            OLEMSGICON messageIcon)
        {
            return ShowMessageBox(messageText, null, messageButtons, defaultButton, messageIcon);
        }

        // <summary>
        //     Helper method to show a message box within the shell.
        // </summary>
        // <param name="messageText">Text to show.</param>
        // <param name="f1Keyword">F1-keyword.</param>
        // <param name="messageButtons">Buttons which should appear in the dialog.</param>
        // <param name="defaultButton">Default button (invoked when user presses return).</param>
        // <param name="messageIcon">Icon (warning, error, informational, etc.) to display</param>
        // <returns>result corresponding to the button clicked by the user.</returns>
        private static DialogResult ShowMessageBox(
            string messageText, string f1Keyword, OLEMSGBUTTON messageButtons,
            OLEMSGDEFBUTTON defaultButton, OLEMSGICON messageIcon)
        {
            var result = 0;
            var uiShell = (IVsUIShell)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsUIShell));

            if (uiShell != null)
            {
                var rclsidComp = Guid.Empty;
                uiShell.ShowMessageBox(
                        0, ref rclsidComp, Resources.App, messageText, f1Keyword, 0, messageButtons, defaultButton, messageIcon, 0, out result);
            }

            return (DialogResult)result;
        }
    }
}
