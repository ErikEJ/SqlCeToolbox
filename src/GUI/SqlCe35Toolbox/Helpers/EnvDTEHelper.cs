using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal class EnvDteHelper
    {
        public Project GetProject()
        {
            return ThreadHelper.JoinableTaskFactory.Run(() => VS.Solutions.GetActiveProjectAsync());
        }

        public static bool IsDebugMode()
        {
            IVsDebugger debugger = ThreadHelper.JoinableTaskFactory.Run(() => VS.Services.GetDebuggerAsync());

            DBGMODE[] mode = new DBGMODE[1];
            ErrorHandler.ThrowOnFailure(debugger.GetMode(mode));

            if (mode[0] != DBGMODE.DBGMODE_Design)
            {
                return true;
            }

            return false;
        }

        public SolutionItem GetProjectDc(Project project, string model, string extension)
        {
            foreach (var item in project.Children)
            {
                if (item.Name.ToLowerInvariant() == model.ToLowerInvariant() + extension)
                {
                    return item;
                }
            }
            return null;
        }

        public SolutionItem GetProjectDataContextClass(Project project, string classFileName)
        {
            foreach (var item in project.Children)
            {
                if (item.Name.ToLowerInvariant() == classFileName)
                {
                    return item;
                }
            }
            return null;
        }

        public static async System.Threading.Tasks.Task AddReferenceAsync(Project project, string reference)
        {
            await project.References.AddAsync(reference);
        }

        public HashSet<string> GetSqlCeFilesInActiveSolution()
        {
            var list = new HashSet<string>();

            var solution = VS.Solutions.GetCurrentSolution();

            if (solution is null)
                return list;

            var projects = ThreadHelper.JoinableTaskFactory.Run(() => VS.Solutions.GetAllProjectsAsync());

            foreach (var item in projects)
            {
                GetSqlCeFilesInProject(item, list);
            }

            return list;
        }

        public string GetInitialFolder()
        {
            var solution = VS.Solutions.GetCurrentSolution();

            if (solution is null)
                return null;

            return Path.GetDirectoryName(solution.FullPath);
        }

        private void GetSqlCeFilesInProject(Project project, HashSet<string> list)
        {
            foreach (var item in project.Children)
            {
                try
                {
                    if (item.Type == SolutionItemType.PhysicalFile)
                    {
                        string path = item.FullPath;
                        foreach (var extension in GetFileExtensions())
                        {
                            if (path.EndsWith(extension, true, CultureInfo.InvariantCulture))
                            {
                                if (!list.Contains(path))
                                    list.Add(path);
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore - see https://github.com/ErikEJ/SqlCeToolbox/issues/842 
                }
            }
        }

        private List<string> GetFileExtensions()
        {
            var list = new List<string>();
            var sdfExtensions = Properties.Settings.Default.FileFilterSqlCe.Split(';');
            foreach (var ext in sdfExtensions)
            {
                list.Add(ext.Replace("*", string.Empty));
            }
            var sqliteExtensions = Properties.Settings.Default.FileFilterSqlite.Split(';');
            foreach (var ext in sqliteExtensions)
            {
                list.Add(ext.Replace("*", string.Empty));
            }
            return list;
        }

        public static Guid VbProject = new Guid("{F184B08F-C81C-45F6-A57F-5ABD9991F28F}");

        public static void LaunchUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        public bool ContainsAllowed(Project project)
        {
            foreach (var id in AllowedProjectKinds)
            {
                if (ThreadHelper.JoinableTaskFactory.Run(() => project.IsKindAsync(id.ToString())))
                {
                    return true;
                }
            }

            return false;
        }

        public List<Guid> AllowedProjectKinds
        {
            // http://www.mztools.com/Articles/2008/MZ2008017.aspx

            get
            {
                return new List<Guid>
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
                    new Guid("{D954291E-2A0B-460D-934E-DC6B0785DB48}"),
                    //.NET Core project (Project.json)
                    new Guid("{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}"),
                    //.NET C# (SDK project format)
                    new Guid("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}")
                };
            }
        }

        // <summary>
        //     Helper method to show an error message within the shell.  This should be used
        //     instead of MessageBox.Show();
        // </summary>
        // <param name="errorText">Text to display.</param>
        public static void ShowError(string errorText)
        {
            VS.MessageBox.ShowError(errorText);
        }

        public static bool ShowMessage(string messageText)
        {
            return VS.MessageBox.ShowConfirm(messageText);
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
            var uiShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));

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
