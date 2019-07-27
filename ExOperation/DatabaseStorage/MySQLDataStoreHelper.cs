using MySql.Data.MySqlClient;
using System;
using System.Net;

namespace ExOperation.DatabaseStorage
{
    public class MySQLDatabaseConnection : IDisposable
    {
        protected MySqlConnection Connection;
        protected bool Disposed = false;

        protected IPAddress DatabaseIP;
        protected string dbUser;
        protected string dbPasswd;
        protected string Database;

        public MySQLDatabaseConnection(IPAddress serverIP, string databaseName)
        {
            DatabaseIP = serverIP;
            Database = databaseName;
        }

        public void Connect()
        {
            if (string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPasswd))
                throw new InvalidOperationException("Username/Password isn't set, please input atleast one instance of string.");
            if (Connection != null)
                throw new InvalidOperationException("Connection already opened");

            string conn = string.Format("Server={0};Port=3306;Database={1};Uid={2};Pwd={3};", DatabaseIP.ToString(), Database, dbUser, dbPasswd);
            Connection = new MySqlConnection(conn);
            Connection.Open();
        }

        public MySqlDataReader IssueCommand(string command)
        {
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.CommandText = command;
            sqlCommand.Connection = Connection;
            MySqlDataReader iRead = sqlCommand.ExecuteReader();
            return iRead;
        }

        public string SearchForString(string command, string collumn)
        {
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.CommandText = command;
            sqlCommand.Connection = Connection;
            MySqlDataReader iRead = sqlCommand.ExecuteReader();
            if (iRead.Read())
            {
                string retString = iRead.GetString(iRead.GetOrdinal(collumn));
                return retString;
            }
            return string.Empty;
        }

        public int SearchForInt(string command, string collumn)
        {
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.CommandText = command;
            sqlCommand.Connection = Connection;
            MySqlDataReader iRead = sqlCommand.ExecuteReader();
            if (iRead.Read())
            {
                int retInt = iRead.GetInt32(iRead.GetOrdinal(collumn));
                return retInt;
            }
            return 0;
        }

        public void Close()
        {
            if (Connection == null)
                throw new InvalidOperationException("Connection is not opened");

            Connection.Close();
            Dispose();
        }

        public string Username
        {
            set
            {
                dbUser = value;
            }
        }

        public string Password
        {
            set
            {
                dbPasswd = value;
            }
        }

        public string GetDatabase()
        {
            return Database;
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                DatabaseIP = null;
                Database = null;
                if (Connection != null)
                    Connection = null;
                if (dbUser != null)
                    dbUser = null;
                if (dbPasswd != null)
                    dbPasswd = null;
                Disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
