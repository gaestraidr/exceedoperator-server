using OperationLibrary.DatabaseStorage;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OperationLibrary.UserManagement
{
    public static class UserInfoInserter
    {
        public static void AddUser(string username, string password)
        {
            AddUser(username, password, string.Empty);
        }

        public static void AddUser(string username, string password, string group)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("Username cannot be empty");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("Password cannot be empty");
            }

            string args = string.Format("-M -N{0} {1}", string.IsNullOrEmpty(group) ? string.Empty : " -G " + group, username);
            Process proc = new Process();
            proc.StartInfo.FileName = "useradd";
            proc.StartInfo.Arguments = args;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            proc.WaitForExit();
            proc.Close();

            proc = new Process();
            proc.StartInfo.FileName = "/root/passwd.sh";
            proc.StartInfo.Arguments = string.Format("{0} {1}", username, password);
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            proc.WaitForExit();
            proc.Close();
            proc.Dispose();
        }

        public static void DeleteUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("Username cannot be empty");
            }

            Process proc = new Process();
            proc.StartInfo.FileName = "deluser";
            proc.StartInfo.Arguments = username;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = false;
            proc.Start();

            proc.WaitForExit();
            proc.Close();
            proc.Dispose();
        }
        
        public static void ChangeUserPassword(string username, string newPassword)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "/root/passwd.sh";
            proc.StartInfo.Arguments = string.Format("{0} {1}", username, newPassword);
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = false;
            proc.Start();

            proc.WaitForExit();
            proc.Close();
            proc.Dispose();
        }

        public static int AddGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new ArgumentNullException("Group cannot be empty");
            }

            int gid = CheckForAvailabilityGID();

            Process proc = new Process();
            proc.StartInfo.FileName = "addgroup";
            proc.StartInfo.Arguments = string.Format("GID{0}", gid.ToString());
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = false;
            proc.Start();

            proc.WaitForExit();
            proc.Close();
            proc.Dispose();

            return gid;
        }

        public static void RemoveGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new ArgumentNullException("Group cannot be empty");
            }

            int gid = CheckForGroupGID(group);

            if (gid == -1)
                throw new EntryPointNotFoundException("Groupname couldn't be identified.");

            Process proc = new Process();
            proc.StartInfo.FileName = "delgroup";
            proc.StartInfo.Arguments = "GID" + gid.ToString();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = false;
            proc.Start();

            proc.WaitForExit();
            proc.Close();
            proc.Dispose();
        }

        public static void AddUserToGroup(string username, string group)
        {
            if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("Username / Group cannot be empty");
            }

            int gid = CheckForGroupGID(group);

            if (gid == -1)
                throw new EntryPointNotFoundException("Groupname couldn't be identified.");

            Process proc = new Process();
            proc.StartInfo.FileName = "gpasswd";
            proc.StartInfo.Arguments = string.Format("-a {0} {1}", username, "GID" + gid.ToString());
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            proc.WaitForExit();
            proc.Close();
            proc.Dispose();
        }

        public static void RemoveUserFromGroup(string username, string group)
        {
            if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("Username / Group cannot be empty");
            }

            int gid = CheckForGroupGID(group);

            if (gid == -1)
                throw new EntryPointNotFoundException("Groupname couldn't be identified.");

            Process proc = new Process();
            proc.StartInfo.FileName = "gpasswd";
            proc.StartInfo.Arguments = string.Format("-d {0} {1}", username, "GID" + gid.ToString());
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            proc.WaitForExit();
            proc.Close();
            proc.Dispose();
        }

        public static int CheckForAvailabilityGID()
        {
            int retint = 0;
            using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
            {
                dbCon.Username = "exceed";
                dbCon.Password = "xenodrom";
                dbCon.Connect();

                string command = "SELECT `groupid` FROM `groupDB`";
                string[] gather = dbCon.GetListCollumnValue(command, "groupid");

                if (gather.Length != 0)
                {
                    List<string> check = new List<string>(gather);
                    for (int i = 0; i < gather.Length; i++)
                    {
                        if (!check.Contains(retint.ToString()))
                        {
                            break;
                        }
                        else retint++;
                   }
                }
                command = string.Empty;
                dbCon.Close();
            }
            return retint;
        }

        public static int CheckForGroupGID(string group)
        {
            int retint = -1;
            using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
            {
                dbCon.Username = "exceed";
                dbCon.Password = "xenodrom";
                dbCon.Connect();

                string command = string.Format("SELECT `groupid` FROM `groupDB` WHERE `groupname`='{0}'", group);
                retint = dbCon.SearchForInt(command, "groupid");

                command = string.Empty;
                dbCon.Close();
            }
            return retint;
        }

    }
}
