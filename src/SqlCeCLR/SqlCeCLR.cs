using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Microsoft.SqlServer.Server;
     
namespace ErikEJ.SqlCe
{
	public static class ClrAccess
	{
        [SqlProcedure()]
        public static void GetTable(string connectionString, string tableName)
        {
            var metaCount = 0;
            var fieldNames = new List<string>();
            //--use: "Provider=Microsoft.SQLSERVER.MOBILE.OLEDB.3.0;OLE DB Services=-4;" for SQL Compact 3.1
            //--use: "Provider=Microsoft.SQLSERVER.CE.OLEDB.3.5;OLE DB Services=-4;" for SQL Compact 3.5 SP2
            //--use: "Provider=Microsoft.SQLSERVER.CE.OLEDB.4.0;OLE DB Services=-4;" for SQL Compact 4.0
            using (var conn = new OleDbConnection(connectionString))
            {
                conn.Open();

                // determine the number of SqlMetadata parameters needed
                using (var cmd = new OleDbCommand())
                {
                    cmd.CommandText = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @p1 ORDER BY ORDINAL_POSITION";
                    cmd.Parameters.Add(new OleDbParameter("@p1", OleDbType.VarWChar, 128));
                    cmd.Parameters[0].Value = tableName;
                    cmd.Connection = conn;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr != null && rdr.Read())
                        {
                            //if (SqlContext.Pipe != null) SqlContext.Pipe.Send(rdr[1].ToString());
                            if (rdr[1].ToString() == "ntext" || rdr[1].ToString() == "image") continue;
                            metaCount++;
                            fieldNames.Add("[" + rdr[0] + "]");
                        }
                    }
                }
                if (metaCount == 0)
                {
                    if (SqlContext.Pipe != null) SqlContext.Pipe.Send("No data found, or table does not exist");
                    return;
                }

                //Get the meta data for the fields
                var metadata = GetMetaData(metaCount, tableName, conn);
                var record = new SqlDataRecord(metadata);
                var fields = new System.Text.StringBuilder();
                foreach (var field in fieldNames)
                {
                    fields.Append(field);
                    fields.Append(", ");
                }
                fields.Remove(fields.Length - 2, 2);

                using (var cmd = new OleDbCommand("SELECT " + fields + " FROM [" + tableName + "]", conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (SqlContext.Pipe != null)
                        {
                            //SqlContext.Pipe.Send(cmd.CommandText);
                            SqlContext.Pipe.SendResultsStart(record);
                            while (rdr != null && rdr.Read())
                            {
                                for (var i = 0; i < rdr.FieldCount; i++)
                                {
                                    if (rdr.IsDBNull(i))
                                    {
                                        record.SetDBNull(i);
                                    }
                                    else
                                    {
                                        if (metadata[i].SqlDbType == SqlDbType.Bit)
                                        {
                                            record.SetBoolean(i, rdr.GetBoolean(i));
                                        }
                                        if (metadata[i].SqlDbType == SqlDbType.TinyInt)
                                        {
                                            record.SetByte(i, rdr.GetByte(i));
                                        }
                                        if (metadata[i].SqlDbType == SqlDbType.SmallInt)
                                        {
                                            record.SetInt16(i, rdr.GetInt16(i));
                                        }
                                        if (metadata[i].SqlDbType == SqlDbType.Int)
                                        {
                                            record.SetInt32(i, rdr.GetInt32(i));
                                        }
                                        if (metadata[i].SqlDbType == SqlDbType.BigInt)
                                        {
                                            record.SetInt64(i, rdr.GetInt64(i));
                                        }
                                        if (metadata[i].SqlDbType == SqlDbType.NVarChar || metadata[i].SqlDbType == SqlDbType.NChar)
                                        {
                                            record.SetString(i, rdr.GetString(i));
                                        }
                                        if (metadata[i].SqlDbType == SqlDbType.UniqueIdentifier)
                                        {
                                            record.SetGuid(i, rdr.GetGuid(i));
                                        }
                                        if (metadata[i].SqlDbType == SqlDbType.Timestamp || metadata[i].SqlDbType == SqlDbType.Binary || metadata[i].SqlDbType == SqlDbType.VarBinary)
                                        {
                                            var tsbuffer = (byte[])rdr[i];
                                            record.SetBytes(i, 0, tsbuffer, 0, tsbuffer.Length);
                                        }
                                        if (metadata[i].SqlDbType == SqlDbType.DateTime)
                                        {
                                            record.SetDateTime(i, rdr.GetDateTime(i));
                                        }
                                        if (metadata[i].SqlDbType == SqlDbType.Money || metadata[i].SqlDbType == SqlDbType.Decimal)
                                        {
                                            record.SetDecimal(i, rdr.GetDecimal(i));
                                        }
                                        if (metadata[i].SqlDbType == SqlDbType.Float)
                                        {
                                            record.SetDouble(i, rdr.GetDouble(i));
                                        }
                                        if (metadata[i].SqlDbType == SqlDbType.Real)
                                        {
                                            record.SetSqlSingle(i, Convert.ToSingle(rdr.GetValue(i)));
                                        }

                                    }
                                }
                                //Send the completed record..
                                SqlContext.Pipe.SendResultsRow(record);
                            }
                            if (rdr != null) rdr.Close();
                            SqlContext.Pipe.SendResultsEnd();
                        }
                    }
                }
                conn.Close();
            }
        }

        /// <summary>
        /// Dynamically create the metadata for each field, needed by the SqlRecord
        /// </summary>
        /// <param name="metaCount">Number of fields</param>
        /// <param name="tableName">Name of table to inspect</param>
        /// <param name="conn">An active, open connection</param>
        /// <returns>Array of SqlMetaData</returns>
        private static SqlMetaData[] GetMetaData(int metaCount, string tableName, OleDbConnection conn)
        {
            var metadata = new SqlMetaData[metaCount];
            var y = 0;
            using (var cmd = new OleDbCommand())
            {
                cmd.CommandText = "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @p1 ORDER BY ORDINAL_POSITION";
                cmd.Parameters.Add(new OleDbParameter("@p1", OleDbType.VarWChar, 128));
                cmd.Parameters[0].Value = tableName;
                cmd.Connection = conn;

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr != null && rdr.Read())
                    {
                        if (rdr[1].ToString() == "ntext" || rdr[1].ToString() == "image") continue;
                        switch (rdr[1].ToString())
                        {
                            //Looking at all the types from "SELECT * FROM INFORMATION_SCHEMA.PROVIDER_TYPES"
                            case "smallint":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.SmallInt);
                                y++;
                                break;
                            case "int":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.Int);
                                y++;
                                break;
                            case "real":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.Real);
                                y++;
                                break;
                            case "float":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.Float);
                                y++;
                                break;
                            case "money":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.Money);
                                y++;
                                break;
                            case "bit":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.Bit);
                                y++;
                                break;
                            case "tinyint":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.TinyInt);
                                y++;
                                break;
                            case "bigint":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.BigInt);
                                y++;
                                break;
                            case "uniqueidentifier":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.UniqueIdentifier);
                                y++;
                                break;
                            case "varbinary":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.VarBinary, Convert.ToInt64(rdr[2].ToString()));
                                y++;
                                break;
                            case "binary":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.Binary, Convert.ToInt64(rdr[2].ToString()));
                                y++;
                                break;
                            case "nvarchar":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.NVarChar, Convert.ToInt64(rdr[2].ToString()));
                                y++;
                                break;
                            case "nchar":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.NChar, Convert.ToInt64(rdr[2].ToString()));
                                y++;
                                break;
                            case "numeric":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.Decimal, Convert.ToByte(rdr[3].ToString()), Convert.ToByte(rdr[4].ToString()));
                                y++;
                                break;
                            case "datetime":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.DateTime);
                                y++;
                                break;
                            case "rowversion":
                                metadata[y] = new SqlMetaData(rdr[0].ToString(), SqlDbType.Timestamp);
                                y++;
                                break;
                        }
                    }
                }
            }
            return metadata;
        }
	}
}
