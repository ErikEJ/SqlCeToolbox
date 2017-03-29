using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace ErikEJ.SqlCeScripting
{
    public class SqlServerHelper : ISqlCeHelper
    {
        public string FormatError(Exception ex)
        {
            var exception = ex as SqlException;
            return exception != null ? Helper.ShowErrors(exception) : ex.ToString();    
        }

        public string GetFullConnectionString(string connectionString)
        {
            throw new NotImplementedException();
        }

        public void CompactDatabase(string connectionString)
        {
            throw new NotImplementedException();
        }

        public void CreateDatabase(string connectionString)
        {
            throw new NotImplementedException();
        }

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

        public string PathFromConnectionString(string connectionString)
        {
            var builder = new SqlConnection(connectionString);
            string server;
            var database = builder.Database;
            using (var cmd = new SqlCommand(connectionString))
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT SERVERPROPERTY('ServerName')";
                    conn.Open();
                    server = (string)cmd.ExecuteScalar();
                }
            }
            return server + "." + database;
        }

        public void UpgradeTo40(string connectionString)
        {
            throw new NotImplementedException();
        }

        public SQLCEVersion DetermineVersion(string fileName)
        {
            throw new NotImplementedException();
        }

        public Version IsV35Installed()
        {
            throw new NotImplementedException();
        }

        public Version IsV40Installed()
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

        public void SaveDataConnection(string repositoryConnectionString, string connectionString, string filePath, int dbType)
        {
            throw new NotImplementedException();
        }

        public void DeleteDataConnnection(string repositoryConnectionString, string connectionString)
        {
            throw new NotImplementedException();
        }

        public void UpdateDataConnection(string repositoryConnectionString, string connectionString, string description)
        {
            throw new NotImplementedException();
        }
    }
}
