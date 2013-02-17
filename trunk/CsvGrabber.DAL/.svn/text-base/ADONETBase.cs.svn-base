using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Web;
using CsvGrabber.Core;

namespace CsvGrabber.DAL
{
    /// <summary>
    /// Summary description for ADONETBase
    /// </summary>
    public class ADONETBase
    {
        public string FileName { get; set; }

        /// <summary>
        /// Executes an action using an SQL connection, then gracefully closes the connection.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <summary>
        /// Executes an action using an SQL connection, then gracefully closes the connection.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        protected virtual void Execute(Action<SQLiteConnection> action)
        {
            SQLiteConnection conn = null;
            try {
                conn = GetConnection();
                conn.Open();
                action.Invoke(conn);
            } finally {
                if (conn != null && conn.State == System.Data.ConnectionState.Open) {
                    try {
                        conn.Close();
                    } catch {
                    }
                }
            }
        }

        /// <summary>
        /// Opens an SQLite connection to the configured database file.
        /// </summary>
        /// <returns>An SQLite connection to the configured database file.</returns>
        protected virtual SQLiteConnection GetConnection()
        {
            return GetConnection(FileName);
        }

        /// <summary>
        /// Opens an SQLite connection to a file.
        /// </summary>
        /// <param name="databasePath">The database path.</param>
        /// <returns>An SQLite connection to the specified file.</returns>
        protected static SQLiteConnection GetConnection(string databasePath)
        {
            return new SQLiteConnection(string.Format(Constants.SQLiteConnectionString, databasePath));
        }

        /// <summary>
        /// Ensures the database exists.
        /// </summary>
        /// <returns></returns>
        public bool EnsureExists()
        {
            bool exists = false;
            if (File.Exists(FileName)) {
                exists = true;
            } else if (CreateDatabase()) {
                exists = true;
            }
            return exists;
        }

        /// <summary>
        /// Creates a database and required tables.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Multiple exceptions, check the reference for DirectoryInfo.Create and the FileInfo constructor.</exception>
        public virtual bool CreateDatabase()
        {
            return false;
        }

    }
}