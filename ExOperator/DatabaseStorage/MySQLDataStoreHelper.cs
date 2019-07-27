using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Net;

namespace OperationLibrary.DatabaseStorage
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

        public void ExecuteCommand(string command)
        {
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.CommandText = command;
            sqlCommand.Connection = Connection;
            MySqlDataReader iRead = sqlCommand.ExecuteReader();
            iRead.Close();
        }

        public bool SearchForBoolean(string command, string column)
        {
            bool retBool;
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.CommandText = command;
            sqlCommand.Connection = Connection;
            MySqlDataReader iRead = sqlCommand.ExecuteReader();
            if (!string.IsNullOrEmpty(column))
            {
                if (iRead.Read())
                    try
                    {
                        retBool = iRead.GetBoolean(iRead.GetOrdinal(column));
                    }
                    catch { return false; }
                else retBool = false;
            }
            else
            {
                retBool = iRead.Read();
            }
            iRead.Close();
            return retBool;
        }

        public string SearchForString(string command, string column)
        {
            string retString = string.Empty;
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.CommandText = command;
            sqlCommand.Connection = Connection;
            MySqlDataReader iRead = sqlCommand.ExecuteReader();
            if (iRead.Read())
            {
                retString = iRead.GetString(iRead.GetOrdinal(column));
            }
            iRead.Close();
            return retString;
        }

        public string[] GetListCollumnValue(string command, string column)
        {
            List<string> retStr = new List<string>();
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.CommandText = command;
            sqlCommand.Connection = Connection;
            MySqlDataReader iRead = sqlCommand.ExecuteReader();
            if (iRead.Read())
            {
                string getStr = iRead.GetString(iRead.GetOrdinal(column));
                retStr.Add(getStr);
                while (iRead.Read())
                {
                    getStr = iRead.GetString(iRead.GetOrdinal(column));
                    retStr.Add(getStr);
                }
            }

            iRead.Close();
            return retStr.ToArray();
        }

        public List<string[]> GetAllCollumnValue(string command)
        {
            List<string[]> retStr = new List<string[]>();
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.CommandText = command;
            sqlCommand.Connection = Connection;
            MySqlDataReader iRead = sqlCommand.ExecuteReader();

            while (iRead.Read())
            {
                string[] appendStack = new string[iRead.FieldCount];
                for (int i = 0; i < iRead.FieldCount; i++)
                {
                    appendStack[i] = iRead[i].ToString();
                }
                retStr.Add(appendStack);
            }

            iRead.Close();
            return retStr;
        }

        public int SearchForInt(string command, string column)
        {
            int retint = -1;
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.CommandText = command;
            sqlCommand.Connection = Connection;
            MySqlDataReader iRead = sqlCommand.ExecuteReader();
            if (iRead.Read())
            {
                retint = iRead.GetInt32(iRead.GetOrdinal(column));
            }
            iRead.Close();
            return retint;
        }

        public string GetSqlDateTimeNow(int addhours = 0)
        {
            string datetime = string.Format("{0}-{1}-{2} {3}", DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), DateTime.Now.Day.ToString(),
                DateTime.Now.AddHours(addhours).TimeOfDay.ToString());
            return datetime;
        }

        public void Close()
        {
            if (Connection == null)
                throw new InvalidOperationException("Connection is not opened");

            Connection.Close();
            Connection.Dispose();
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
