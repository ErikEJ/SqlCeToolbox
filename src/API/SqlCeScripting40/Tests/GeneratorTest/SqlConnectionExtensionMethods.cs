using System.Data.SqlClient;

namespace Tests.GeneratorTest
{
    public static class SqlConnectionExtensionMethods
    {
        public static bool DatabaseExists(this SqlConnection conn, string databaseName)
        {
            string commandText = $"SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}'";
            using (SqlCommand cmd = new SqlCommand(commandText, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        public static void CreateDatabase(this SqlConnection conn, string databaseName)
        {
            string commandText = $"CREATE DATABASE [{databaseName}]";
            conn.Execute(commandText);
        }

        public static void DeleteDatabase(this SqlConnection conn, string databaseName)
        {
            string commandText = $"DROP DATABASE [{databaseName}]";
            conn.Execute(commandText);
        }

        public static void Execute(this SqlConnection conn, string commandText)
        {
            using (SqlCommand cmd = new SqlCommand(commandText, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void SetSingleUserMode(this SqlConnection conn, string databaseName)
        {
            string commandText = $"ALTER DATABASE [{databaseName}] set single_user with rollback immediate";
            conn.Execute(commandText);
        }
    }
}
