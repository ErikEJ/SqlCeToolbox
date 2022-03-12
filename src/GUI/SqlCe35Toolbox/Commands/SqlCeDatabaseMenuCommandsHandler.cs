using Community.VisualStudio.Toolkit;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class SqlCeDatabaseMenuCommandsHandler
    {
        private readonly ExplorerToolWindow _parentWindow;
        private readonly SqlCeToolboxPackage package;

        public SqlCeDatabaseMenuCommandsHandler(ExplorerToolWindow parent)
        {
            _parentWindow = parent;
            package = _parentWindow.Package as SqlCeToolboxPackage;
        }

        public void CopyCeDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var path = DataConnectionHelper.GetFilePath(databaseInfo.DatabaseInfo.ConnectionString, databaseInfo.DatabaseInfo.DatabaseType);
                Clipboard.Clear();
                Clipboard.SetData(DataFormats.FileDrop, new[] { path });
                DataConnectionHelper.LogUsage("DatabaseCopy");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType);
            }
        }


#region Maintenance menu items

        public void SetPassword(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var dbInfo = databaseInfo.DatabaseInfo;
                var pwd = new PasswordDialog();
                pwd.ShowModal();
                if (!pwd.DialogResult.HasValue || !pwd.DialogResult.Value) return;
                var helper = Helpers.RepositoryHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                var newConnectionString = helper.ChangeDatabasePassword(databaseInfo.DatabaseInfo.ConnectionString, pwd.Password);
                if (dbInfo.FromServerExplorer)
                {
                    var providerId = Resources.SqlCompact35Provider;
                    if (dbInfo.DatabaseType == DatabaseType.SQLCE40)
                        providerId = Resources.SqlCompact40Provider;
                    DataConnectionHelper.RemoveDataConnection(package, dbInfo.ConnectionString, new Guid(providerId));
                }
                else
                {
                    DataConnectionHelper.RemoveDataConnection(databaseInfo.DatabaseInfo.ConnectionString);
                }

                if (!string.IsNullOrEmpty(newConnectionString))
                {
                    DataConnectionHelper.SaveDataConnection(newConnectionString, dbInfo.DatabaseType, package);
                    EnvDteHelper.ShowMessage("Password was set, and connection updated");
                }
                else
                {
                    EnvDteHelper.ShowMessage("Password was set, but could not update connection, please reconnect the database");
                }

                var control = _parentWindow.Content as ExplorerControl;
                if (control != null) control.BuildDatabaseTree();
                DataConnectionHelper.LogUsage("DatabaseMaintainSetPassword");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void VerifyDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var helper = Helpers.RepositoryHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.VerifyDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Verify completed");
                DataConnectionHelper.LogUsage("DatabaseMaintainVerify");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void RepairDatabaseDeleteCorruptedRows(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var helper = Helpers.RepositoryHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.RepairDatabaseDeleteCorruptedRows(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Repair completed");
                DataConnectionHelper.LogUsage("DatabaseMaintainRepair");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void RepairDatabaseRecoverAllOrFail(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var helper = Helpers.RepositoryHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.RepairDatabaseRecoverAllOrFail(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Repair completed");
                DataConnectionHelper.LogUsage("DatabaseMaintainRepair");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void RepairDatabaseRecoverAllPossibleRows(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var helper = Helpers.RepositoryHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.RepairDatabaseRecoverAllPossibleRows(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Repair completed");
                DataConnectionHelper.LogUsage("DatabaseMaintainRepair");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

#endregion

        public void UpgradeTo40(object sender, ExecutedRoutedEventArgs e)
        {
            if (EnvDteHelper.ShowMessageBox("This will upgrade the 3.5 database to 4.0 format, and leave a renamed backup of the 3.5 database. Do you wish to proceed?",
                OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY) == System.Windows.Forms.DialogResult.No)
                return;

            if (!Helpers.RepositoryHelper.IsV40Installed())
            {
                EnvDteHelper.ShowError("The SQL Server Compact 4.0 runtime is not installed, cannot upgrade. Install the 4.0 runtime.");
                return;
            }
            try
            {
                var databaseInfo = ValidateMenuInfo(sender);
                if (databaseInfo == null) return;

                var helper = new SqlCeHelper4();
                var path = helper.PathFromConnectionString(databaseInfo.DatabaseInfo.ConnectionString);

                if (!File.Exists(path))
                {
                    EnvDteHelper.ShowError(string.Format("Database file in path: {0} could not be found", path));
                    return;
                }
                var path1 = Path.GetDirectoryName(path);
                if (path1 != null)
                {
                    var newFile = Path.Combine(path1, Path.GetFileNameWithoutExtension(path) + "_35" + Path.GetExtension(path));
                    if (File.Exists(newFile))
                    {
                        for (var i = 0; i < 100; i++)
                        {
                            newFile = Path.Combine(path1, Path.GetFileNameWithoutExtension(newFile) + "_" + i.ToString() + "." + Path.GetExtension(newFile));
                            if (!File.Exists(newFile))
                                break;
                        }
                    }

                    if (File.Exists(newFile))
                    {
                        EnvDteHelper.ShowError("Could not create unique file name...");
                        return;
                    }
                    File.Copy(path, newFile);
                    helper.UpgradeTo40(databaseInfo.DatabaseInfo.ConnectionString);
                    EnvDteHelper.ShowMessage(string.Format("Database upgraded, version 3.5 database backed up to: {0}", newFile));
                }
                if (databaseInfo.DatabaseInfo.FromServerExplorer)
                {
                    DataConnectionHelper.RemoveDataConnection(package, databaseInfo.DatabaseInfo.ConnectionString, new Guid(Resources.SqlCompact35Provider));
                }
                else
                {
                    DataConnectionHelper.RemoveDataConnection(databaseInfo.DatabaseInfo.ConnectionString);
                }
                DataConnectionHelper.SaveDataConnection(databaseInfo.DatabaseInfo.ConnectionString, DatabaseType.SQLCE40, package);
                var control = _parentWindow.Content as ExplorerControl;
                if (control != null) control.BuildDatabaseTree();
                DataConnectionHelper.LogUsage("DatabaseUpgrade40");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
            }
        }
#if SSMS
#else
        public async void GenerateDataContextInProject(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (package == null) return;
            if (EnvDteHelper.IsDebugMode())
            {
                EnvDteHelper.ShowError("Cannot generate code while debugging");
                return;                
            }

            var helper = Helpers.RepositoryHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
            if (!helper.IsV35DbProviderInstalled())
            {
                EnvDteHelper.ShowError("This feature requires the SQL Server Compact 3.5 SP2 DbProvider to be properly installed");
                return;                                
            }

            var dteH = new EnvDteHelper();

            var project = dteH.GetProject();
            if (project == null)
            {
                EnvDteHelper.ShowError("Please select a project in Solution Explorer, where you want the DataContext to be placed");
                return;
            }
            if (!dteH.ContainsAllowed(project))
            {
                EnvDteHelper.ShowError("The selected project type does not support LINQ to SQL (please let me know if I am wrong)");
                return;            
            }

            var tfm = ThreadHelper.JoinableTaskFactory.Run(() => project.GetAttributeAsync("TargetFrameworkMoniker"));

            if (string.IsNullOrEmpty(tfm))
            {
                EnvDteHelper.ShowError("No TFM");
                return;
            }

            if (!tfm.Contains(".NETFramework"))
            {
                EnvDteHelper.ShowError("The selected project type does not support .NET Desktop - wrong TargetFrameworkMoniker: " + tfm);
                return;
            }

            var sqlMetalPath = ProbeSqlMetalRegPaths();
            if (string.IsNullOrEmpty(sqlMetalPath))
            {
                EnvDteHelper.ShowError("Could not find SQLMetal file location");
                return;                
            }

            var sdfFileName = string.Empty;

            try
            {
                using (var repository = Helpers.RepositoryHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    var tables = repository.GetAllTableNames();
                    var pks = repository.GetAllPrimaryKeys();
                    var checkedTables = string.Empty;
                    foreach (var tableName in tables)
                    {
                        var pk = pks.Where(k => k.TableName == tableName).FirstOrDefault();
                        if (pk.TableName == null)
                        {
                            checkedTables += tableName + Environment.NewLine;
                        }
                    }
                    if (!string.IsNullOrEmpty(checkedTables))
                    {
                        var message = string.Format("The tables below do not have Primary Keys defined,{0}and will not be generated properly:{1}{2}", Environment.NewLine, Environment.NewLine, checkedTables);
                        EnvDteHelper.ShowError(message);
                    }
                    var dbInfo = repository.GetDatabaseInfo();
                    foreach (var kvp in dbInfo)
                    {
                        if (kvp.Key == "Database")
                        {
                            sdfFileName = kvp.Value;
                            break;
                        }
                    }
                    sdfFileName = Path.GetFileName(sdfFileName);
                }

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(databaseInfo.DatabaseInfo.Caption);
                if (fileNameWithoutExtension != null)
                {
                    var model = fileNameWithoutExtension.Replace(" ", string.Empty).Replace("#", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
                    model = model + "Context";
                    var dcDialog = new DataContextDialog();
                    dcDialog.ModelName = model;
                    dcDialog.IsDesktop = true;
                    dcDialog.ProjectName = project.Name;
                    dcDialog.NameSpace = ThreadHelper.JoinableTaskFactory.Run(() => project.GetAttributeAsync("DefaultNamespace"));
                    
                    if (ThreadHelper.JoinableTaskFactory.Run(() => project.IsKindAsync(ProjectTypes.VB)))
                    {
                        dcDialog.CodeLanguage = "VB";
                    }
                    else
                    {
                        dcDialog.CodeLanguage = "C#";
                    }
                    var result = dcDialog.ShowModal();
                    if (!result.HasValue || result.Value != true || string.IsNullOrWhiteSpace(dcDialog.ModelName))
                        return;
                    if (dcDialog.AddRowversionColumns)
                    {
                        AddRowVersionColumns(databaseInfo);
                    }

                    var sdfPath = databaseInfo.DatabaseInfo.ConnectionString;

                    //If version 4.0, create a 3.5 schema sdf, and use that as connection string
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE40)
                    {
                        var tempFile = Path.GetTempFileName();
                        using (var repository = Helpers.RepositoryHelper.CreateRepository(databaseInfo.DatabaseInfo))
                        {
                            var generator = DataConnectionHelper.CreateGenerator(repository, tempFile, databaseInfo.DatabaseInfo.DatabaseType);
                            generator.ScriptDatabaseToFile(Scope.Schema);
                        }
                        if (sdfFileName != null)
                        {
                            sdfPath = Path.Combine(Path.GetTempPath(), sdfFileName);
                        }
                        using (Stream stream = new MemoryStream(Resources.SqlCe35AddinStore))
                        {
                            // Create a FileStream object to write a stream to a file 
                            using (var fileStream = File.Create(sdfPath, (int)stream.Length))
                            {
                                // Fill the bytes[] array with the stream data 
                                var bytesInStream = new byte[stream.Length];
                                stream.Read(bytesInStream, 0, bytesInStream.Length);
                                // Use FileStream object to write to the specified file 
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                            }
                        }
                        var info = new DatabaseInfo
                        {
                            ConnectionString = "Data Source=" + sdfPath,
                            DatabaseType = DatabaseType.SQLCE35
                        };
                        using (var repository = Helpers.RepositoryHelper.CreateRepository(info))
                        {
                            var script = File.ReadAllText(tempFile);
                            repository.ExecuteSql(script);
                        }
                        sdfPath = info.ConnectionString;
                    }

                    var versionNumber = GetVersionTableNumber(databaseInfo.DatabaseInfo, true);

                    model = dcDialog.ModelName;
                    var dcPath = Path.Combine(Path.GetTempPath(), model + ".cs");
                    if (dcDialog.CodeLanguage == "VB")
                    {
                        dcPath = Path.Combine(Path.GetTempPath(), model + ".vb");
                    }
                    var parameters = " /provider:SQLCompact /code:\"" + dcPath + "\"";
                    parameters += " /conn:\"" + sdfPath + "\"";
                    parameters += " /context:" + model;
                    if (dcDialog.Pluralize)
                    {
                        parameters += " /pluralize";
                    }
                    if (!string.IsNullOrWhiteSpace(dcDialog.NameSpace))
                    {
                        parameters += " /namespace:" + dcDialog.NameSpace;
                    }
                    var dcH = new DataContextHelper();

                    var sqlmetalResult = dcH.RunSqlMetal(sqlMetalPath, parameters);
                    if (!File.Exists(dcPath))
                    {
                        EnvDteHelper.ShowError("Error during SQL Metal run: " + sqlmetalResult);
                        return;
                    }

                    if (dcDialog.MultipleFiles)
                    {
                        var classes = DataContextHelper.SplitIntoMultipleFiles(dcPath, dcDialog.NameSpace, model);
                        var projectPath = Path.GetDirectoryName(project.FullPath);

                        foreach (var item in classes)
                        {
                            var fileName = Path.Combine(projectPath, item.Key + ".cs");
                            if (File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                            File.WriteAllText(fileName, item.Value);
                            var classItem = dteH.GetProjectDataContextClass(project, fileName);
                            if (classItem != null)
                            {
                                File.Delete(classItem.FullPath);
                            }
                            ThreadHelper.JoinableTaskFactory.Run(() =>  project.AddExistingFilesAsync(fileName));
                        }

                    }
                    else
                    {
                        var extension = ".cs";
                        if (dcDialog.CodeLanguage == "VB")
                            extension = ".vb";
                        var dcItem = dteH.GetProjectDc(project, model, extension);
                        //TODO Test!
                        var dcTarget = Path.Combine(Path.GetDirectoryName(project.FullPath), model + extension);
                        if (dcItem == null)
                        {
                            File.Copy(dcPath, dcTarget);
                            ThreadHelper.JoinableTaskFactory.Run(() => project.AddExistingFilesAsync(dcTarget));
                        }
                        else
                        {
                            if (EnvDteHelper.ShowMessageBox("The Data Context class already exists in the project, do you wish to replace it?", OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY) == System.Windows.Forms.DialogResult.Yes) 
                            {
                                File.Copy(dcPath, dcTarget, true);
                                ThreadHelper.JoinableTaskFactory.Run(() => project.AddExistingFilesAsync(dcTarget));
                            }
                        }
                    }
                    await EnvDteHelper.AddReferenceAsync(project, "System.Data.Linq");
                    if (dcDialog.AddConnectionStringBuilder)
                    {
                        var projectPath = Path.GetDirectoryName(project.FullPath);

                        var fileName = "LocalDatabaseConnectionStringBuilder.cs";

                        var filePath = Path.Combine(projectPath, fileName);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        using (Stream stream = new MemoryStream(Resources.LocalDatabaseConnectionStringBuilder))
                        {
                            // Create a FileStream object to write a stream to a file 
                            using (var fileStream = File.Create(filePath, (int)stream.Length))
                            {
                                // Fill the bytes[] array with the stream data 
                                var bytesInStream = new byte[stream.Length];
                                stream.Read(bytesInStream, 0, bytesInStream.Length);
                                // Use FileStream object to write to the specified file 
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                            }
                        }
                        ThreadHelper.JoinableTaskFactory.Run(() => project.AddExistingFilesAsync(filePath));
                    }

                    // Creates __Version table and adds one row if desired
                    if (dcDialog.AddVersionTable)
                    {
                        using (var repository = Helpers.RepositoryHelper.CreateRepository(databaseInfo.DatabaseInfo))
                        {
                            var list = repository.GetAllTableNames();
                            if (!list.Contains("__VERSION"))
                            {
                                repository.ExecuteSql(string.Format(@"
                                CREATE TABLE [__VERSION] (
                                  [SchemaVersion] int NOT NULL
                                , [DateUpdated] datetime NOT NULL DEFAULT (GETDATE())
                                );
                                GO
                                CREATE INDEX [IX_SchemaVersion] ON [__VERSION] ([SchemaVersion] DESC);
                                GO
                                INSERT INTO [__VERSION] ([SchemaVersion]) VALUES ({0});
                                GO", versionNumber));
                            }
                        }

                    }
                    DataConnectionHelper.LogUsage("DatabaseCreateDC");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        private string ProbeSqlMetalRegPaths()
        {
            var paths = new List<string>
            {
                @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\sqlmetal.exe",
                @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.2 Tools\sqlmetal.exe",
                @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\sqlmetal.exe",
                @"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6 Tools\sqlmetal.exe"
            };

            var sqlMetalRegPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\NETFXSDK\4.8\WinSDK-NetFx40Tools", "InstallationFolder", string.Empty);
            if (!string.IsNullOrEmpty(sqlMetalRegPath))
            {
                paths.Add(Path.Combine(sqlMetalRegPath, "sqlmetal.exe"));
            }

            sqlMetalRegPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v10.0A", "InstallationFolder", string.Empty);
            if (!string.IsNullOrEmpty(sqlMetalRegPath))
            {
                paths.Add(Path.Combine(sqlMetalRegPath, "bin\\NETFX 4.6.2 Tools", "sqlmetal.exe"));
            }

            sqlMetalRegPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v10.0A", "InstallationFolder", string.Empty);
            if (!string.IsNullOrEmpty(sqlMetalRegPath))
            {
                paths.Add(Path.Combine(sqlMetalRegPath, "bin\\NETFX 4.6.1 Tools", "sqlmetal.exe"));
            }

            sqlMetalRegPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v10.0A", "InstallationFolder", string.Empty);
            if (!string.IsNullOrEmpty(sqlMetalRegPath))
            {
                paths.Add(Path.Combine(sqlMetalRegPath, "bin\\NETFX 4.6 Tools", "sqlmetal.exe"));
            }

            sqlMetalRegPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v8.1A", "InstallationFolder", string.Empty);
            if (!string.IsNullOrEmpty(sqlMetalRegPath))
            {
                paths.Add(Path.Combine(sqlMetalRegPath, "bin\\NETFX 4.5.1 Tools", "sqlmetal.exe"));
            }

            sqlMetalRegPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v8.0A", "InstallationFolder", string.Empty);
            if (!string.IsNullOrEmpty(sqlMetalRegPath))
            {
                paths.Add(Path.Combine(sqlMetalRegPath, "bin\\NETFX 4.0 Tools", "sqlmetal.exe"));
            }

            sqlMetalRegPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A\WinSDK-NetFx40Tools", "InstallationFolder", string.Empty);

            if (!string.IsNullOrEmpty(sqlMetalRegPath))
            {
                paths.Add(Path.Combine(sqlMetalRegPath, "sqlmetal.exe"));
            }

            return paths.FirstOrDefault(path => File.Exists(path));
        }

#endif

        private int GetVersionTableNumber(DatabaseInfo databaseInfo, bool isDesktop)
        {
            if (isDesktop)
                return 0;

            var version = 0;
            using (var repository = Helpers.RepositoryHelper.CreateRepository(databaseInfo))
            {
                var list = repository.GetAllTableNames();
                if (list.Contains("__VERSION"))
                {
                    var ds = repository.ExecuteSql(@"
                                SELECT MAX([SchemaVersion]) FROM __VERSION;
                                GO");
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        version = int.Parse(ds.Tables[0].Rows[0][0].ToString());
                    }

                    repository.ExecuteSql(@"
                                DROP TABLE [__VERSION];
                                GO");
                }
            }

            return version;
        }

        public void SyncFxDeprovisionDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDteHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }
            if (!SyncFxHelper.IsProvisioned(databaseInfo.DatabaseInfo))
            {
                EnvDteHelper.ShowError("The database is not provisioned, cannot deprovision");
                return;                
            }
            try
            {
                new SyncFxHelper().DeprovisionDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                databaseInfo.ExplorerControl.RefreshTables(databaseInfo.DatabaseInfo);
                EnvDteHelper.ShowMessage("Database deprovisioned");
                DataConnectionHelper.LogUsage("DatabaseSyncDeprovision");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }

        }

        public void SyncFxGenerateSnapshot(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDteHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }
            if (!SyncFxHelper.IsProvisioned(databaseInfo.DatabaseInfo))
            {
                EnvDteHelper.ShowError("The database is not provisioned, cannot generate snapshots");
                return;
            }

            var fd = new SaveFileDialog
            {
                Title = "Save generated snapshot database file as",
                Filter = DataConnectionHelper.GetSqlCeFileFilter(),
                OverwritePrompt = true,
                ValidateNames = true
            };
            var result = fd.ShowDialog();
            if (!result.HasValue || !result.Value) return;
            var fileName = fd.FileName;
            try
            {
                SyncFxHelper.GenerateSnapshot(databaseInfo.DatabaseInfo.ConnectionString, fileName);
                EnvDteHelper.ShowMessage("Database snapshot generated.");
                DataConnectionHelper.LogUsage("DatabaseSyncSnapshot");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35, false);
            }
        }

        public void SyncFxProvisionScope(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDteHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }

            try
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(databaseInfo.DatabaseInfo.Caption);
                if (fileNameWithoutExtension == null) return;
                var model = fileNameWithoutExtension.Replace(" ", string.Empty).Replace("#", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
                var sfd = new SyncFxDialog {ModelName = model};

                var res = sfd.ShowModal();

                if (!res.HasValue || res.Value != true || (sfd.Tables.Count <= 0)) return;
                if (SyncFxHelper.SqlCeScopeExists(databaseInfo.DatabaseInfo.ConnectionString, model))
                {
                    EnvDteHelper.ShowError("Scope name is already in use. Please enter a different scope name.");
                    return;
                }

                model = sfd.ModelName;
                new SyncFxHelper().ProvisionScope(databaseInfo.DatabaseInfo.ConnectionString, model, sfd.Columns.Where(c => sfd.Tables.Contains(c.TableName)).ToList());
                EnvDteHelper.ShowMessage("Scope: " + model + " has been provisioned.");
                DataConnectionHelper.LogUsage("DatabaseSyncProvision");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public async void SyncFxGenerateSyncCodeInProject(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            if (package == null) return;

            var dteH = new EnvDteHelper();

            var project = dteH.GetProject();
            if (project == null)
            {
                EnvDteHelper.ShowError("Please select a project in Solution Explorer, where you want the SyncFx classes to be placed");
                return;
            }
            if (!dteH.ContainsAllowed(project))
            {
                EnvDteHelper.ShowError("The selected project type does not support Sync Framework (please let me know if I am wrong)");
                return;
            }

            var tfm = ThreadHelper.JoinableTaskFactory.Run(() => project.GetAttributeAsync("TargetFrameworkMoniker"));

            if (string.IsNullOrEmpty(tfm))
            {
                EnvDteHelper.ShowError("The selected project type does not support Sync Framework - missing TargetFrameworkMoniker");
                return;
            }
            if (!tfm.Contains(".NETFramework"))
            {
                EnvDteHelper.ShowError("The selected project type does not support .NET Desktop - wrong TargetFrameworkMoniker: " + tfm);
                return;
            }
            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDteHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }

            try
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(databaseInfo.DatabaseInfo.Caption);
                if (fileNameWithoutExtension != null)
                {
                    var model = fileNameWithoutExtension.Replace(" ", string.Empty).Replace("#", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
                    var sfd = new SyncFxDialog {ModelName = model};

                    var res = sfd.ShowModal();
                    if (!res.HasValue || res.Value != true || (sfd.Tables.Count <= 0)) return;
                    model = sfd.ModelName;
                    var defaultNamespace = ThreadHelper.JoinableTaskFactory.Run(() => project.GetAttributeAsync("DefaultNamespace"));

                    var classes = new SyncFxHelper().GenerateCodeForScope(string.Empty, databaseInfo.DatabaseInfo.ConnectionString, "SQLCE", model, sfd.Columns.Where(c => sfd.Tables.Contains(c.TableName)).ToList(), defaultNamespace);
                    var projectPath = Path.GetDirectoryName(project.FullPath);

                    foreach (var item in classes)
                    {
                        var fileName = Path.Combine(projectPath, item.Key + ".cs");
                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }
                        File.WriteAllText(fileName, item.Value);
                        ThreadHelper.JoinableTaskFactory.Run(() =>  project.AddExistingFilesAsync(fileName));
                    }
                    //Adding references - http://blogs.msdn.com/b/murat/archive/2008/07/30/envdte-adding-a-refernce-to-a-project.aspx
                    await EnvDteHelper.AddReferenceAsync(project, "System.Data.SqlServerCe, Version=3.5.1.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");

                    await EnvDteHelper.AddReferenceAsync(project, "Microsoft.Synchronization, Version=2.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    await EnvDteHelper.AddReferenceAsync(project, "Microsoft.Synchronization.Data, Version=3.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    await EnvDteHelper.AddReferenceAsync(project, "Microsoft.Synchronization.Data.SqlServer, Version=3.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    await EnvDteHelper.AddReferenceAsync(project, "Microsoft.Synchronization.Data.SqlServerCe, Version=3.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    EnvDteHelper.ShowMessage("Scope: " + model + " code generated.");
                    DataConnectionHelper.LogUsage("DatabaseSyncCodegen");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        private static void AddRowVersionColumns(DatabaseMenuCommandParameters databaseInfo)
        {
            using (var repository = Helpers.RepositoryHelper.CreateRepository(databaseInfo.DatabaseInfo))
            {
                var list = repository.GetAllTableNames();
                var allColumns = repository.GetAllColumns();
                foreach (var table in list)
                {
                    if (table.StartsWith("__")) continue;
                    var rowVersionCol = allColumns.SingleOrDefault(c => c.TableName == table && c.DataType == "rowversion");
                    if (rowVersionCol == null)
                    {
                        repository.ExecuteSql(string.Format("ALTER TABLE {0} ADD COLUMN VersionColumn rowversion NOT NULL;{1}GO", table, Environment.NewLine));
                    }
                }
            }
        }

        private static DatabaseMenuCommandParameters ValidateMenuInfo(object sender)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                return menuItem.CommandParameter as DatabaseMenuCommandParameters;
            }
            return null;
        }
    }
}
