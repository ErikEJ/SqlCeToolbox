using System;
using System.Data.SqlClient;

namespace Tests.GeneratorTest
{
    internal class TempDatabase : IDisposable
    {
        private string databaseName;
        private SqlConnection sqlConnection;

        public TempDatabase(SqlConnection sqlConnection, string databaseName)
        {
            this.sqlConnection = sqlConnection;
            this.databaseName = databaseName;

            sqlConnection.CreateDatabase(databaseName);
            this.ConnectionString = $"{sqlConnection.ConnectionString}Initial Catalog={databaseName};";
        }

        public string ConnectionString { get; private set; }

        public void Dispose()
        {
            try
            {
                sqlConnection.SetSingleUserMode(databaseName);

                sqlConnection.DeleteDatabase(databaseName);
            }
            catch (Exception ex)
            {
                // do not raise exceptions because this is called in a dispose method and may swallow other exceptions
                Console.WriteLine($"Failed to delete database: {this.databaseName}{Environment.NewLine}{ex}");
            }
        }
    }
}