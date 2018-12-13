using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace ErikEJ.SqlCeScripting
{
    public class RepositoryHelper
    {
        // Contrib from hugo on CodePlex - thanks!
        public static List<Constraint> GetGroupForeignKeys(List<Constraint> foreignKeys, List<string> allTables)
        {
            var groupedForeignKeys = new List<Constraint>();

            var uniqueTables = (from c in foreignKeys
                                select c.ConstraintTableName).Distinct().ToList();
            int i = 1;
            foreach (string tableName in uniqueTables)
            {
                {
                    var uniqueConstraints = (from c in foreignKeys
                                             where c.ConstraintTableName == tableName
                                             select c.ConstraintName).Distinct().ToList();
                    foreach (string item in uniqueConstraints)
                    {
                        string value = item;
                        var constraints = foreignKeys.Where(c => c.ConstraintName.Equals(value, System.StringComparison.Ordinal) && c.ConstraintTableName == tableName).ToList();

                        if (constraints.Count == 1)
                        {
                            Constraint constraint = constraints[0];
                            constraint.Columns.Add(constraint.ColumnName);
                            constraint.UniqueColumns.Add(constraint.UniqueColumnName);
                            var found = groupedForeignKeys.Where(fk => fk.ConstraintName == constraint.ConstraintName && fk.ConstraintTableName != constraint.ConstraintTableName).Any();
                            if (found)
                            {
                                constraint.ConstraintName = constraint.ConstraintName + i.ToString();
                                i++;
                            }
                            else
                            {
                                var tfound = allTables.Where(ut => ut == constraint.ConstraintName).Any();
                                if (tfound)
                                {
                                    constraint.ConstraintName = constraint.ConstraintName + i.ToString();
                                    i++;
                                }
                            }

                            groupedForeignKeys.Add(constraint);
                        }
                        else
                        {
                            var newConstraint = new Constraint { ConstraintTableName = constraints[0].ConstraintTableName, ConstraintName = constraints[0].ConstraintName, UniqueConstraintTableName = constraints[0].UniqueConstraintTableName, UniqueConstraintName = constraints[0].UniqueConstraintName, DeleteRule = constraints[0].DeleteRule, UpdateRule = constraints[0].UpdateRule, Columns = new ColumnList(), UniqueColumns = new ColumnList() };
                            foreach (Constraint c in constraints)
                            {
                                newConstraint.Columns.Add(c.ColumnName);
                                newConstraint.UniqueColumns.Add(c.UniqueColumnName);
                            }
                            var found = groupedForeignKeys.Where(fk => fk.ConstraintName == newConstraint.ConstraintName && fk.ConstraintTableName != newConstraint.ConstraintTableName).Any();
                            if (found)
                            {
                                newConstraint.ConstraintName = newConstraint.ConstraintName + i.ToString();
                                i++;
                            }
                            groupedForeignKeys.Add(newConstraint);
                        }
                    }
                }
            }
            return groupedForeignKeys;
        }

        // Returns the human-readable file size for an arbitrary, 64-bit file size
        //  The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        // http://www.somacon.com/p576.php
        public static string GetSizeReadable(long i)
        {
            string sign = (i < 0 ? "-" : "");
            double readable = (i < 0 ? -i : i);
            string suffix;
            if (i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (double)(i >> 50);
            }
            else if (i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (double)(i >> 40);
            }
            else if (i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (double)(i >> 30);
            }
            else if (i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (double)(i >> 20);
            }
            else if (i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (double)(i >> 10);
            }
            else if (i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = (double)i;
            }
            else
            {
                return i.ToString(sign + "0 B"); // Byte
            }
            readable = readable / 1024;

            NumberFormatInfo nfi = (NumberFormatInfo)
            CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = "";

            return sign + readable.ToString("0.### ") + suffix;
        }

        public static CommandExecute FindExecuteType(string commandText)
        {
            if (string.IsNullOrEmpty(commandText.Trim()))
            {
                return CommandExecute.Undefined;
            }
            string test = commandText.Trim();

            while (test.StartsWith(Environment.NewLine))
            {
                test = test.Remove(0, 2);
            }
            //Remove initial comment lines if applicable
            while (test.StartsWith("--"))
            {
                int pos = test.IndexOf("\r\n", 0);
                if (pos > 0)
                {
                    test = test.Substring(pos + 2);
                    while (test.StartsWith(Environment.NewLine))
                    {
                        test = test.Remove(0, 2);
                    }
                }
                else
                {
                    break;
                }
            }
            while (test.StartsWith(Environment.NewLine))
            {
                test = test.Remove(0, 2);
            }
            if (string.IsNullOrWhiteSpace(test))
            {
                return CommandExecute.Undefined;
            }
            if (test.StartsWith("--"))
            {
                return CommandExecute.Undefined;
            }
            if (test.ToUpperInvariant().StartsWith("SELECT", StringComparison.Ordinal) && test.Length > 6 && char.IsWhiteSpace(test[6]))
            {
                return CommandExecute.DataTable;
            }
            if (test.ToUpperInvariant().StartsWith("EXPLAIN QUERY PLAN ", StringComparison.Ordinal))
            {
                return CommandExecute.DataTable;
            }
            if (test.ToUpperInvariant().StartsWith("PRAGMA", StringComparison.Ordinal) && test.Length > 6 && char.IsWhiteSpace(test[6]))
            {
                return CommandExecute.DataTable;
            }
            if (test.ToUpperInvariant().StartsWith("WITH", StringComparison.Ordinal) && test.Length > 4 && char.IsWhiteSpace(test[4]))
            {
                return CommandExecute.DataTable;
            }
            if (test.ToUpperInvariant().StartsWith("SP_", StringComparison.Ordinal))
            {
                return CommandExecute.DataTable;
            }
            if (test.ToUpperInvariant().StartsWith("CREATE ") || test.ToUpperInvariant().StartsWith("ALTER ") || test.ToUpperInvariant().StartsWith("DROP "))
            {
                return CommandExecute.NonQuerySchemaChanged;
            }
            else
            {
                return CommandExecute.NonQuery;
            }
        }

    }
}
