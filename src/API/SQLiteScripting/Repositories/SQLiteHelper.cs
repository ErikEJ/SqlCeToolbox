using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErikEJ.SqlCeScripting;
using System.Data.SQLite;

namespace ErikEJ.SQLiteScripting
{
    public class SqliteHelper : ISqlCeHelper
    {
        #region ISqlCeHelper Members

        public void SaveDataConnection(string repositoryConnectionString, string connectionString, string filePath, int dbType)
        {
            using (var cmd = new SQLiteCommand(repositoryConnectionString))
            {
                var conn = new SQLiteConnection(repositoryConnectionString);
                cmd.Connection = conn;
                cmd.CommandText = "INSERT INTO Databases (Source, FileName, CeVersion) VALUES (@Source, @FileName, @CeVersion)";
                cmd.Parameters.Add("@Source", System.Data.DbType.String, 2048);
                cmd.Parameters.Add("@FileName", System.Data.DbType.String, 512);
                cmd.Parameters.Add("@CeVersion", System.Data.DbType.Int32);

                cmd.Parameters[0].Value = connectionString;
                cmd.Parameters[1].Value = filePath;
                cmd.Parameters[2].Value = dbType;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateDataConnection(string repositoryConnectionString, string connectionString, string description)
        {
            using (var cmd = new SQLiteCommand(repositoryConnectionString))
            {
                var conn = new SQLiteConnection(repositoryConnectionString);
                cmd.Connection = conn;
                cmd.CommandText = "UPDATE Databases SET FileName = @FileName WHERE Source = @Source";
                cmd.Parameters.Add("@Source", System.Data.DbType.String, 2048);
                cmd.Parameters.Add("@FileName", System.Data.DbType.String, 512);

                cmd.Parameters[0].Value = connectionString;
                cmd.Parameters[1].Value = description;
                conn.Open();
                cmd.ExecuteNonQuery();
            }            
        }

        public void DeleteDataConnnection(string repositoryConnectionString, string connectionString)
        {
            using (var cmd = new SQLiteCommand())
            {
                var conn = new SQLiteConnection(repositoryConnectionString);
                cmd.Connection = conn;
                cmd.CommandText = "DELETE FROM Databases WHERE Source = @Source;";
                cmd.Parameters.Add("@Source", System.Data.DbType.String, 2048);
                cmd.Parameters[0].Value = connectionString;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }



        public string FormatError(Exception e)
        {
            if (e.GetType() == typeof(SQLiteException))
            {
                SQLiteException ex = e as SQLiteException;
                StringBuilder bld = new StringBuilder();
                Exception inner = ex.InnerException;

                if (null != inner)
                {
                    bld.Append("Inner Exception: " + inner.ToString());
                }
                bld.AppendLine("ErrorCode : " + ex.ErrorCode);
                bld.AppendLine("Message   : " + ex.Message);
                bld.AppendLine("Result    : " + Enum.GetName(typeof(SQLiteErrorCode), ex.ResultCode));
                return bld.ToString();        

            }
            else return e.ToString();
        }

        public string GetFullConnectionString(string connectionString)
        {
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder(connectionString);
            return sb.ConnectionString;
        }

        public void CreateDatabase(string connectionString)
        {
            SQLiteConnection.CreateFile(PathFromConnectionString(connectionString));
        }

        public string PathFromConnectionString(string connectionString)
        {
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder(connectionString);
            return sb.DataSource;
        }

        public void CompactDatabase(string connectionString)
        {
            using (var cmd = new SQLiteCommand())
            {
                var conn = new SQLiteConnection(connectionString);
                cmd.Connection = conn;
                cmd.CommandText = "VACUUM;";
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region Not implemented
        public void VerifyDatabase(string connectionString)
        {
            throw new NotImplementedException();
        }

        public string ChangeDatabasePassword(string connectionString, string password)
        {
            throw new NotImplementedException();
        }

        public void RepairDatabaseRecoverAllPossibleRows(string connectionString)
        {
            throw new NotImplementedException();
        }

        public void RepairDatabaseRecoverAllOrFail(string connectionString)
        {
            throw new NotImplementedException();
        }

        public void RepairDatabaseDeleteCorruptedRows(string connectionString)
        {
            throw new NotImplementedException();
        }

        public void ShrinkDatabase(string connectionString)
        {
            throw new NotImplementedException();
        }

        public void UpgradeTo40(string connectionString)
        {
            throw new NotImplementedException();
        }

        public SQLCEVersion DetermineVersion(string fileName)
        {
            throw new NotImplementedException();
        }

        public bool IsV35Installed()
        {
            throw new NotImplementedException();
        }

        public bool IsV40Installed()
        {
            throw new NotImplementedException();
        }

        public bool IsV35DbProviderInstalled()
        {
            throw new NotImplementedException();
        }

        public bool IsV40DbProviderInstalled()
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
