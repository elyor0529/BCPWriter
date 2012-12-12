﻿namespace BCPWriter
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// MS SQL Server backend for BCPWriter, allows to easily debug BCPWriter.
    /// </summary>
    /// <remarks>
    /// If BCPWriter.BackendMode equals Debug (instead of Normal which is the default),
    /// BCPWriter will also send all datas to MS SQL Server instead of just writing .bcp files.<br/>
    /// <br/>
    /// BCPWriter will create a table (and a database) named BCPTest and insert all the rows
    /// into it. Then BCPWriter will run bcp out in order to get the .bcp files
    /// from MS SQL Server.<br/>
    /// The idea is to compare the .bcp files generated by BCPWriter and the ones
    /// generated by bcp out, this way it is easier to find bugs.
    /// </remarks>
    class BCPWriterSQLServer
    {
        /// <summary>
        /// Initializes the MS SQL Server backend.
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        /// <param name="columns">columns</param>
        /// <param name="rows">rows</param>
        public BCPWriterSQLServer(BinaryWriter writer, List<IBCPSerialization> columns, IEnumerable<object> rows)
        {
            if (columns.Count() == 0)
            {
                throw new ArgumentException("No columns");
            }

            string createTableString = GetCreateTableString(columns);
            string insertIntoString = GetInsertIntoString(columns, rows);

            SendSQLRequests(createTableString, insertIntoString);
            GenerateBCPFileFromSQLServer(writer);
        }

        /// <summary>
        /// Gets the string to create the SQL table.
        /// </summary>
        /// 
        /// <remarks>
        /// <example>
        /// <code>
        /// CREATE TABLE BCPTest (
        /// col0 xml,
        /// col1 xml,
        /// col2 nvarchar(max),
        /// col3 varbinary(max)
        /// )
        /// </code>
        /// </example>
        /// </remarks>
        /// 
        /// <param name="columns">List of SQL types</param>
        /// <returns>SQL create table string</returns>
        static private string GetCreateTableString(IEnumerable<IBCPSerialization> columns)
        {
            StringBuilder createTableString = new StringBuilder();
            createTableString.AppendLine("CREATE TABLE BCPTest (");

            int columnNumber = 0;
            foreach (IBCPSerialization column in columns)
            {
                createTableString.AppendFormat("col{0} ", columnNumber++);

                // FIXME Is there a better way than casting every type?
                // Don't forget to add new SQL types here and to modify the unit tests accordingly
                if (column is SQLBinary)
                {
                    SQLBinary sql = (SQLBinary)column;
                    createTableString.AppendFormat("binary({0})", sql.Length);
                }
                else if (column is SQLChar)
                {
                    SQLChar sql = (SQLChar)column;
                    createTableString.AppendFormat("char({0})", sql.Length);
                }
                else if (column is SQLInt)
                {
                    SQLInt sql = (SQLInt)column;
                    createTableString.Append("int");
                }
                else if (column is SQLNChar)
                {
                    SQLNChar sql = (SQLNChar)column;
                    createTableString.AppendFormat("nchar({0})", sql.Length);
                }
                else if (column is SQLNVarChar)
                {
                    SQLNVarChar sql = (SQLNVarChar)column;
                    if (sql.Length == SQLNVarChar.MAX)
                    {
                        createTableString.Append("nvarchar(max)");
                    }
                    else
                    {
                        createTableString.AppendFormat("nvarchar({0})", sql.Length);
                    }
                }
                else if (column is SQLVarBinary)
                {
                    SQLVarBinary sql = (SQLVarBinary)column;
                    if (sql.Length == SQLVarBinary.MAX)
                    {
                        createTableString.Append("varbinary(max)");
                    }
                    else
                    {
                        createTableString.AppendFormat("varbinary({0})", sql.Length);
                    }
                }
                else if (column is SQLVarChar)
                {
                    SQLVarChar sql = (SQLVarChar)column;
                    if (sql.Length == SQLVarChar.MAX)
                    {
                        createTableString.Append("varchar(max)");
                    }
                    else
                    {
                        createTableString.AppendFormat("varchar({0})", sql.Length);
                    }
                }
                else if (column is SQLNText)
                {
                    SQLNText sql = (SQLNText)column;
                    createTableString.Append("ntext");
                }
                else if (column is SQLText)
                {
                    SQLText sql = (SQLText)column;
                    createTableString.Append("text");
                }
                else if (column is SQLXml)
                {
                    SQLXml sql = (SQLXml)column;
                    createTableString.Append("xml");
                }
                else if (column is SQLReal)
                {
                    SQLReal sql = (SQLReal)column;
                    createTableString.Append("real");
                }
                else if (column is SQLFloat)
                {
                    SQLFloat sql = (SQLFloat)column;
                    createTableString.Append("float");
                }
                else if (column is SQLUniqueIdentifier)
                {
                    SQLUniqueIdentifier sql = (SQLUniqueIdentifier)column;
                    createTableString.Append("uniqueidentifier");
                }
                else if (column is SQLBigInt)
                {
                    SQLBigInt sql = (SQLBigInt)column;
                    createTableString.Append("bigint");
                }
                else if (column is SQLDateTime)
                {
                    SQLDateTime sql = (SQLDateTime)column;
                    createTableString.Append("datetime");
                }
                else if (column is SQLDateTime2)
                {
                    SQLDateTime2 sql = (SQLDateTime2)column;
                    createTableString.Append("datetime2");
                }
                else if (column is SQLDate)
                {
                    SQLDate sql = (SQLDate)column;
                    createTableString.Append("date");
                }
                else if (column is SQLTime)
                {
                    SQLTime sql = (SQLTime)column;
                    createTableString.Append("time");
                }
                else
                {
                    System.Diagnostics.Trace.Assert(false);
                }

                if (columnNumber < columns.Count())
                {
                    createTableString.AppendLine(",");
                }
            }

            createTableString.Append(")");

            return createTableString.ToString();
        }

        /// <summary>
        /// Gets the string to insert values into the SQL table.
        /// </summary>
        /// 
        /// <remarks>
        /// <example>
        /// <code>
        /// INSERT INTO BCPTest VALUES
        /// (
        /// 'string',
        /// 'string',
        /// NULL,
        /// NULL
        /// ),
        /// (
        /// 'string',
        /// 'string',
        /// NULL,
        /// NULL
        /// )
        /// </code>
        /// </example>
        /// </remarks>
        /// 
        /// <param name="columns">List of SQL types</param>
        /// <param name="rows">Values</param>
        /// <returns>SQL insert into table string</returns>
        static private string GetInsertIntoString(List<IBCPSerialization> columns, IEnumerable<object> rows)
        {
            StringBuilder insertIntoString = new StringBuilder();

            if (rows.Count() == 0)
            {
                return string.Empty;
            }

            insertIntoString.AppendLine("INSERT INTO BCPTest VALUES");

            for (int i = 0; i < rows.Count(); i++)
            {
                int modulo = i % columns.Count();
                if (modulo == 0 && i > 0)
                {
                    insertIntoString.AppendLine("),");
                }
                if (modulo > 0)
                {
                    insertIntoString.AppendLine(",");
                }
                if (modulo == 0)
                {
                    insertIntoString.Append("(");
                }

                IBCPSerialization column = columns[modulo];
                object row = rows.ElementAt(i);

                // FIXME Is there a better way than casting every type?
                // Don't forget to add new SQL types here and to modify the unit tests accordingly
                if (column is SQLBinary)
                {
                    SQLBinary sql = (SQLBinary)column;
                    byte[] value = (byte[])row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat(
                            "CAST('{0}' AS binary({1}))",
                            Encoding.Default.GetString(value), sql.Length);
                    }
                }
                else if (column is SQLChar)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLInt)
                {
                    int? value = (int?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("{0}", value.Value);
                    }
                }
                else if (column is SQLNChar)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLNVarChar)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLVarBinary)
                {
                    SQLVarBinary sql = (SQLVarBinary)column;
                    byte[] value = (byte[])row;

                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        if (sql.Length == SQLVarBinary.MAX)
                        {
                            insertIntoString.AppendFormat(
                                "CAST('{0}' AS varbinary(max))",
                                Encoding.Default.GetString(value));
                        }
                        else
                        {
                            insertIntoString.AppendFormat(
                                "CAST('{0}' AS varbinary({1}))",
                                Encoding.Default.GetString(value), sql.Length);
                        }
                    }
                }
                else if (column is SQLVarChar)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLNText)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLText)
                {
                    string value = (string)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value);
                    }
                }
                else if (column is SQLXml)
                {
                    XmlDocument value = (XmlDocument)row;
                    if (value == null)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.DocumentElement.OuterXml);
                    }
                }
                else if (column is SQLReal)
                {
                    float? value = (float?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("{0}", value.Value);
                    }
                }
                else if (column is SQLFloat)
                {
                    if (row is float)
                    {
                        // Don't treat null case here
                        float? value = (float?)row;
                        insertIntoString.AppendFormat("{0}", value.Value);
                    }
                    else
                    {
                        // If we don't know, let's cast it to double
                        // if value is null then double? will work, not float?
                        // More explanations inside SQLFloat
                        double? value = (double?)row;
                        if (!value.HasValue)
                        {
                            insertIntoString.Append("NULL");
                        }
                        else
                        {
                            insertIntoString.AppendFormat("{0}", value.Value);
                        }
                    }
                }
                else if (column is SQLUniqueIdentifier)
                {
                    Guid? value = (Guid?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.Value);
                    }
                }
                else if (column is SQLBigInt)
                {
                    long? value = (long?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("{0}", value.Value);
                    }
                }
                else if (column is SQLDateTime)
                {
                    DateTime? value = (DateTime?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.Value);
                    }
                }
                else if (column is SQLDateTime2)
                {
                    DateTime? value = (DateTime?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.Value);
                    }
                }
                else if (column is SQLDate)
                {
                    DateTime? value = (DateTime?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.Value);
                    }
                }
                else if (column is SQLTime)
                {
                    DateTime? value = (DateTime?)row;
                    if (!value.HasValue)
                    {
                        insertIntoString.Append("NULL");
                    }
                    else
                    {
                        insertIntoString.AppendFormat("'{0}'", value.Value);
                    }
                }
                else
                {
                    System.Diagnostics.Trace.Assert(false);
                }
            }

            insertIntoString.Append(")");

            return insertIntoString.ToString();
        }

        /// <summary>
        /// Sends the SQL requests (create table + insert into table).
        /// </summary>
        /// <param name="createTableString">SQL create table string</param>
        /// <param name="insertIntoString">SQL insert into table string</param>
        static private void SendSQLRequests(string createTableString, string insertIntoString)
        {
            const string server = "localhost";
            const string username = "sa";
            const string password = "Password01";
            const string database = "BCPTest";

            string connectionString = string.Format(
                "Data Source={0};User ID={1};Password={2}",
                server, username, password);

            SqlCommand command = new SqlCommand();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    command.Connection = connection;

                    // Create database if needed
                    command.CommandText = "IF DB_ID('BCPTest') IS NULL CREATE DATABASE BCPTest";
                    command.ExecuteNonQuery();

                    connection.Close();
                }

                connectionString = string.Format(
                    "Data Source={0};User ID={1};Password={2};Initial Catalog={3}",
                    server, username, password, database);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    command.Connection = connection;

                    // Drop table if any
                    command.CommandText = "IF OBJECT_ID('BCPTest', 'U') IS NOT NULL DROP TABLE BCPTest";
                    command.ExecuteNonQuery();

                    // Create table
                    if (!string.IsNullOrEmpty(createTableString))
                    {
                        command.CommandText = createTableString;
                        command.ExecuteNonQuery();
                    }

                    // Insert values
                    if (!string.IsNullOrEmpty(insertIntoString))
                    {
                        command.CommandText = insertIntoString;
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                // Could not connect
                string msg = e.Message;
            }
        }

        /// <summary>
        /// Runs bcp out inside the values from MS SQL Server.
        /// </summary>
        /// <param name="writer">BinaryWriter</param>
        static private void GenerateBCPFileFromSQLServer(BinaryWriter writer)
        {
            // Default file name
            string baseFileName = "BCPTest.bcp";

            Stream stream = writer.BaseStream;
            if (stream is FileStream)
            {
                FileStream fileStream = (FileStream)stream;
                baseFileName = fileStream.Name;
            }

            string bcpFileName = string.Format("{0}.{1}", baseFileName, "BCPTest");

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                                                                {
                                                                    UseShellExecute = false,
                                                                    RedirectStandardOutput = true,
                                                                    RedirectStandardInput = true,
                                                                    RedirectStandardError = true,
                                                                    FileName = "bcp",
                                                                    Arguments =
                                                                        "[BCPTest].[dbo].[BCPTest] out " + bcpFileName +
                                                                        " -S localhost -U sa -P Password01 -n"
                                                                };

            try
            {
                // Start the process with the info we specified
                // Call WaitForExit and then the using statement will close
                using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
                {
                    string errorMessage = process.StandardError.ReadToEnd();
                    string outputMessage = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();
                }
            }
            catch (Exception e)
            {
                // Couldn't sent the request
                string msg = e.Message;
            }
        }
    }
}
