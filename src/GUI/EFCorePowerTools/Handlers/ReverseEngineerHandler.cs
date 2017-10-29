using EnvDTE;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ReverseEngineer20;

namespace EFCorePowerTools.Handlers
{
    internal class ReverseEngineerHandler
    {
        private readonly EFCorePowerToolsPackage _package;

        public ReverseEngineerHandler(EFCorePowerToolsPackage package)
        {
            _package = package;
        }

        public void GenerateServerDgmlFiles()
        {
            try
            {
                if (_package.Dte2.Mode == vsIDEMode.vsIDEModeDebug)
                {
                    EnvDteHelper.ShowError("Cannot generate code while debugging");
                    return;
                }

                var databaseList = EnvDteHelper.GetDataConnections(_package);

                var psd = new PickServerDatabaseDialog(databaseList, _package);
                var diagRes = psd.ShowModal();
                if (!diagRes.HasValue || !diagRes.Value) return;

                _package.Dte2.StatusBar.Text = "Loading schema information...";

                var dbInfo = psd.SelectedDatabase.Value;

                if (dbInfo.DatabaseType == DatabaseType.SQLCE35)
                {
                    EnvDteHelper.ShowError($"Unsupported provider: {dbInfo.ServerVersion}");
                    return;
                }

                var ptd = new PickTablesDialog();
                using (var repository = RepositoryHelper.CreateRepository(dbInfo))
                {
                    ptd.Tables = repository.GetAllTableNamesForExclusion();
                }

                var res = ptd.ShowModal();
                if (!res.HasValue || !res.Value) return;

                var path = Path.GetTempFileName() + ".dgml";

                using (var repository = RepositoryHelper.CreateRepository(dbInfo))
                {
                    var generator = RepositoryHelper.CreateGenerator(repository, path, dbInfo.DatabaseType);
                    generator.GenerateSchemaGraph(dbInfo.ConnectionString, ptd.Tables);
                    _package.Dte2.ItemOperations.OpenFile(path);
                    _package.Dte2.ActiveDocument.Activate();
                }
                Telemetry.TrackEvent("PowerTools.GenerateSchemaDgml");
            }
            catch (Exception ex)
            {
                _package.LogError(new List<string>(), ex);
            }
        }

        public async void ReverseEngineerCodeFirst(Project project)
        {
            try
            {
                if (_package.Dte2.Mode == vsIDEMode.vsIDEModeDebug)
                {
                    EnvDteHelper.ShowError("Cannot generate code while debugging");
                    return;
                }

                var startTime = DateTime.Now;

                var databaseList = EnvDteHelper.GetDataConnections(_package);

                var psd = new PickServerDatabaseDialog(databaseList, _package);
                var diagRes = psd.ShowModal();
                if (!diagRes.HasValue || !diagRes.Value) return;

                // Show dialog with SqlClient selected by default
                _package.Dte2.StatusBar.Text = "Loading schema information...";

                var dbInfo = psd.SelectedDatabase.Value;

                if (dbInfo.DatabaseType == DatabaseType.SQLCE35)
                {
                    EnvDteHelper.ShowError($"Unsupported provider: {dbInfo.ServerVersion}");
                    return;
                }

                var ptd = new PickTablesDialog { IncludeTables = true };
                using (var repository = RepositoryHelper.CreateRepository(dbInfo))
                {
                    ptd.Tables = repository.GetAllTableNamesForExclusion();
                }

                var res = ptd.ShowModal();
                if (!res.HasValue || !res.Value) return;

                var classBasis = RepositoryHelper.GetClassBasis(dbInfo.ConnectionString, dbInfo.DatabaseType);

                var dteH = new EnvDteHelper();
                var revEng = new EfCoreReverseEngineer();

                var model = revEng.GenerateClassName(classBasis) + "Context";
                var packageResult = dteH.ContainsEfCoreReference(project, dbInfo.DatabaseType);

                var modelDialog = new EfCoreModelDialog
                {
                    InstallNuGetPackage = !packageResult.Item1,
                    ModelName = model,
                    ProjectName = project.Name,
                    NameSpace = project.Properties.Item("DefaultNamespace").Value.ToString()
                };

                _package.Dte2.StatusBar.Text = "Getting options...";
                var result = modelDialog.ShowModal();
                if (!result.HasValue || result.Value != true)
                    return;

                var projectPath = project.Properties.Item("FullPath").Value.ToString();

                var options = new ReverseEngineerOptions
                {
                    UseFluentApiOnly = !modelDialog.UseDataAnnotations,
                    ConnectionString = dbInfo.ConnectionString,
                    ContextClassName = modelDialog.ModelName,
                    DatabaseType = (ReverseEngineer20.DatabaseType)dbInfo.DatabaseType,
                    ProjectPath = projectPath,
                    OutputPath = modelDialog.OutputPath,
                    ProjectRootNamespace = modelDialog.NameSpace,
                    UseDatabaseNames = modelDialog.UseDatabaseNames,
                    UseInflector =  modelDialog.UsePluralizer,
                    Tables = ptd.Tables
                };

                _package.Dte2.StatusBar.Text = "Generating code...";
                var revEngResult = revEng.GenerateFiles(options);

                if (modelDialog.SelectedTobeGenerated == 0 || modelDialog.SelectedTobeGenerated == 2)
                {
                    foreach (var filePath in revEngResult.EntityTypeFilePaths)
                    {
                        project.ProjectItems.AddFromFile(filePath);
                    }
                }
                if (modelDialog.SelectedTobeGenerated == 0 || modelDialog.SelectedTobeGenerated == 1)
                {
                    project.ProjectItems.AddFromFile(revEngResult.ContextFilePath);
                    _package.Dte2.ItemOperations.OpenFile(revEngResult.ContextFilePath);
                }

                packageResult = dteH.ContainsEfCoreReference(project, dbInfo.DatabaseType);

                var missingProviderPackage = packageResult.Item1 ? null : packageResult.Item2;
                if (modelDialog.InstallNuGetPackage)
                {
                    missingProviderPackage = null;
                }

                _package.Dte2.StatusBar.Text = "Reporting result...";
                var errors = ReportRevEngErrors(revEngResult, missingProviderPackage);

                if (modelDialog.InstallNuGetPackage)
                {
                    _package.Dte2.StatusBar.Text = "Installing EF Core provider package";
                    var nuGetHelper = new NuGetHelper();
                    await nuGetHelper.InstallPackageAsync(packageResult.Item2, project);
                }
                var duration = DateTime.Now - startTime;
                _package.Dte2.StatusBar.Text = $"Reverse engineer completed in {duration:h\\:mm\\:ss}";

                EnvDteHelper.ShowMessage(errors);

                if (revEngResult.EntityErrors.Count > 0)
                {
                    _package.LogError(revEngResult.EntityErrors, null);
                }
                if (revEngResult.EntityWarnings.Count > 0)
                {
                    _package.LogError(revEngResult.EntityWarnings, null);
                }
                Telemetry.TrackEvent("PowerTools.ReversEngineer");
            }
            catch (AggregateException ae)
            {
                foreach (var innerException in ae.Flatten().InnerExceptions)
                {
                    _package.LogError(new List<string>(), innerException);
                }
            }
            catch (Exception exception)
            {
                _package.LogError(new List<string>(), exception);
            }
        }

        private string ReportRevEngErrors(EfCoreReverseEngineerResult revEngResult, string missingProviderPackage)
        {
            var errors = new StringBuilder();
            if (revEngResult.EntityErrors.Count == 0)
            {
                errors.Append("Model generated successfully." + Environment.NewLine);
            }
            else
            {
                errors.Append("Please check the output window for errors" + Environment.NewLine);
            }

            if (revEngResult.EntityWarnings.Count > 0)
            {
                errors.Append("Please check the output window for warnings" + Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(missingProviderPackage))
            {
                errors.AppendLine();
                errors.AppendFormat("The \"{0}\" NuGet package was not found in the project - it must be installed in order to build.", missingProviderPackage);
            }

            return errors.ToString();
        }
    }
}