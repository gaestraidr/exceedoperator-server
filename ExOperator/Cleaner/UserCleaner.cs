using OperationLibrary.DatabaseStorage;
using OperationLibrary.OperatorHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OperationLibrary.Cleaner
{
    internal class UserCleaner
    {
        public static void RemoveUserInEveryProject(string username)
        {
            string confUser = "[U] " + username;
            ProjectCollection projCol = ProjectBrowserStore.GetListProject();

            if (projCol.Count == 0)
                return;

            foreach (Project proj in projCol)
            {
                try
                {
                    ProjectBrowserStore.RemoveMemberFromProject(confUser, proj);
                    MySQLAccountParsing.RemoveMemberFromProject(confUser, proj);
                }
                catch (Exception ex)
                {
                    if (!(ex is MissingMemberException))
                    {
                        throw;
                    }
                    else continue;
                }
            }
        }

        public static void RemoveUserInEveryGroup(string username)
        {
            string[] groupstack = new string[0];
            using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
            {
                dbCon.Username = "exceed";
                dbCon.Password = "xenodrom";
                dbCon.Connect();

                string command = "SELECT `groupname` FROM `groupDB`";
                groupstack = dbCon.GetListCollumnValue(command, "groupname");

                command = string.Empty;
                dbCon.Close();
            }

            if (groupstack.Length == 0)
                return;

            foreach (string group in groupstack)
            {
                try
                {
                    MySQLAccountParsing.RemoveUserFromGroup(username, group);
                }
                catch (Exception ex)
                {
                    if (!(ex is MissingMemberException))
                    {
                        throw;
                    }
                    else continue;
                }
            }
        }
    }
}
