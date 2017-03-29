
using System;
using System.Data.SqlServerCe;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

// ReSharper disable once CheckNamespace
namespace ErikEJ.SqlCeScripting
{
#if V40 
    public class SqlCeHelper4 : ISqlCeHelper 
#else
    public class SqlCeHelper : ISqlCeHelper 
#endif
    {
        public string FormatError(Exception ex)
        {
            if (ex == null) return string.Empty;
            if (ex.GetType() == typeof (SqlCeException)) return Helper.ShowErrors((SqlCeException) ex);
            var exception = ex as SqlException;
            return exception != null ? Helper.ShowErrors(exception) : ex.ToString();
        }

        public string GetFullConnectionString(string connectionString)
        {
            using (SqlCeReplication repl = new SqlCeReplication())
            {
                repl.SubscriberConnectionString = connectionString;
                return repl.SubscriberConnectionString;
            }
        }

        public void CreateDatabase(string connectionString)
        {
            using (SqlCeEngine engine = new SqlCeEngine(connectionString))
            {
                engine.CreateDatabase();
            }
        }

        public void ShrinkDatabase(string connectionString)
        {
            using (SqlCeEngine engine = new SqlCeEngine(connectionString))
            {
                engine.Shrink();
            }
        }

        public void CompactDatabase(string connectionString)
        {
            using (SqlCeEngine engine = new SqlCeEngine(connectionString))
            {
                engine.Compact(null);
            }
        }

        public void RepairDatabaseDeleteCorruptedRows(string connectionString)
        {
            using (SqlCeEngine engine = new SqlCeEngine(connectionString))
            {
                engine.Repair(connectionString, RepairOption.DeleteCorruptedRows);
            }
        }

        public void RepairDatabaseRecoverAllOrFail(string connectionString)
        {
            using (SqlCeEngine engine = new SqlCeEngine(connectionString))
            {
                engine.Repair(connectionString, RepairOption.RecoverAllOrFail);
            }
        }

        public void RepairDatabaseRecoverAllPossibleRows(string connectionString)
        {
            using (SqlCeEngine engine = new SqlCeEngine(connectionString))
            {
                engine.Repair(connectionString, RepairOption.RecoverAllPossibleRows);
            }
        }

        public void VerifyDatabase(string connectionString)
        {
            using (SqlCeEngine engine = new SqlCeEngine(connectionString))
            {
                engine.Verify(VerifyOption.Enhanced);
            }
        }

        public string ChangeDatabasePassword(string connectionString, string password)
        {
            if (password == null)
            {
                password = string.Empty;
            }
            using (SqlCeEngine engine = new SqlCeEngine(connectionString))
            {
                engine.Compact(string.Format("Data Source=;Password={0}", password));
            }
#if V40
            var builder = new SqlCeConnectionStringBuilder(connectionString);
            builder.Password = password;
            return builder.ConnectionString;
#else
            return string.Empty;
#endif
        }

        public void SaveDataConnection(string repositoryConnectionString, string connectionString, string filePath, int dbType)
        {
            using (var cmd = new SqlCeCommand(repositoryConnectionString))
            {
                var conn = new SqlCeConnection(repositoryConnectionString);
                cmd.Connection = conn; 
                cmd.CommandText = "INSERT INTO Databases (Source, FileName, CeVersion) VALUES (@Source, @FileName, @CeVersion)";
                cmd.Parameters.Add("@Source", System.Data.SqlDbType.NVarChar, 2048);
                cmd.Parameters.Add("@FileName", System.Data.SqlDbType.NVarChar, 512);
                cmd.Parameters.Add("@CeVersion", System.Data.SqlDbType.Int);

                cmd.Parameters[0].Value = connectionString;
                cmd.Parameters[1].Value = filePath;
                cmd.Parameters[2].Value = dbType;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateDataConnection(string repositoryConnectionString, string connectionString, string description)
        {
            using (var cmd = new SqlCeCommand(repositoryConnectionString))
            {
                var conn = new SqlCeConnection(repositoryConnectionString);
                cmd.Connection = conn;
                cmd.CommandText = "UPDATE Databases SET FileName = @FileName WHERE Source = @Source";
                cmd.Parameters.Add("@Source", System.Data.SqlDbType.NVarChar, 2048);
                cmd.Parameters.Add("@FileName", System.Data.SqlDbType.NVarChar, 512);

                cmd.Parameters[0].Value = connectionString;
                cmd.Parameters[1].Value = description;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteDataConnnection(string repositoryConnectionString, string connectionString)
        {
            using (var cmd = new SqlCeCommand())
            {
                var conn = new SqlCeConnection(repositoryConnectionString);
                cmd.Connection = conn; 
                cmd.CommandText = "DELETE FROM Databases WHERE Source = @Source;";
                cmd.Parameters.Add("@Source", System.Data.SqlDbType.NVarChar, 2048);
                cmd.Parameters[0].Value = connectionString;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

#if V40
        public void UpgradeTo40(string connectionString)
        {
            string filename;
            using (SqlCeConnection conn = new SqlCeConnection(connectionString))
            {
                filename = conn.Database;
            }
            if (filename.Contains("|DataDirectory|"))
                throw new ApplicationException("DataDirectory macro not supported for upgrade");

            SQLCEVersion fileversion = DetermineVersion(filename);
            if (fileversion == SQLCEVersion.SQLCE20)
                throw new ApplicationException("Unable to upgrade from 2.0 to 4.0");

            if (SQLCEVersion.SQLCE40 > fileversion)
            {
                SqlCeEngine engine = new SqlCeEngine(connectionString);
                engine.Upgrade();
            }
        }

        public string PathFromConnectionString(string connectionString)
        {
            SqlCeConnectionStringBuilder sb = new SqlCeConnectionStringBuilder(connectionString);
            return sb.DataSource;
        }

#else
        public void UpgradeTo40(string connectionString)
        {
            throw new NotImplementedException("Not implemented");
        }

        public string PathFromConnectionString(string connectionString)
        {
            using (SqlCeConnection conn = new SqlCeConnection(GetFullConnectionString(connectionString)))
            {
                return conn.DataSource;
            }
        }
#endif

        public SQLCEVersion DetermineVersion(string fileName)
        {
            var versionDictionary = new System.Collections.Generic.Dictionary<int, SQLCEVersion> 
        { 
            { 0x73616261, SQLCEVersion.SQLCE20 }, 
            { 0x002dd714, SQLCEVersion.SQLCE30},
            { 0x00357b9d, SQLCEVersion.SQLCE35},
            { 0x003d0900, SQLCEVersion.SQLCE40}
        };
            int versionLongword;
            using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open))
            {
                fs.Seek(16, System.IO.SeekOrigin.Begin);
                using (System.IO.BinaryReader reader = new System.IO.BinaryReader(fs))
                {
                    versionLongword = reader.ReadInt32();
                }
            }
            if (versionDictionary.ContainsKey(versionLongword))
            {
                return versionDictionary[versionLongword];
            }
            throw new ApplicationException("Unable to determine database file version");
        }

        public bool IsV40DbProviderInstalled()
        {
            try
            {
                System.Data.Common.DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
            }
            catch
            {
                return false;
            }
            return true;
        }

        public Version IsV40Installed()
        {
            try
            {
                var assembly = System.Reflection.Assembly.Load("System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                if (!assembly.GlobalAssemblyCache)
                    return null;
                if (assembly.GetName().Version.ToString(2) != "4.0")
                    return null;

                using (
                    var rk =
                        Registry.LocalMachine.OpenSubKey(
                            @"SOFTWARE\Microsoft\Microsoft SQL Server Compact Edition\v4.0",
                            RegistryKeyPermissionCheck.ReadSubTree))
                {
                    var value = rk?.GetValue("NativeDir");
                    if (value != null)
                    {
                        var path = Path.Combine(value.ToString(), "sqlceme40.dll");
                        if (!File.Exists(path))
                        {
                            return null;
                        }
                    }
                }
                if (assembly.Location == null) return null;
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return new Version(fvi.FileVersion);
            }
            catch
            {
                return null;
            }
        }

        public Version IsV35Installed()
        {
            try
            {
                var assembly = System.Reflection.Assembly.Load("System.Data.SqlServerCe, Version=3.5.1.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                if (!assembly.GlobalAssemblyCache)
                    return null;
                if (assembly.GetName().Version.ToString(2) != "3.5")
                    return null;
                if (assembly.Location == null) return null;
                var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                var version = new Version(fvi.FileVersion);
                return version >= new Version(3, 5, 8080) ? version : null;
            }
            catch
            {
                return null;
            }
        }

        public bool IsV35DbProviderInstalled()
        {
            try
            {
                System.Data.Common.DbProviderFactories.GetFactory("System.Data.SqlServerCe.3.5");
            }
            catch
            {
                return false;
            }
            return true;
        }

    }
}
