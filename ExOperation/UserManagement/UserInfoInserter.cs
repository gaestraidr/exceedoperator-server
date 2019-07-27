using ExOperation.PacketProcessor;
using Mono.Unix;
using System;
using System.Diagnostics;

namespace ExOperation.UserManagement
{
    public static class UserInfoInserter
    {
        public static void AddUser(string username, string group = null)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("Username cannot be empty");
            }
            string args = string.Format("-m{0} {1}", string.IsNullOrEmpty(group) ? string.Empty : " -G " + group, username);
            Process proc = new Process();
            proc.StartInfo.FileName = "useradd";
            proc.StartInfo.Arguments = args;
            proc.Start();
            proc.WaitForExit();

            string result = proc.StandardOutput.ReadToEnd();
            if (!result.Contains("Created"))
            {
                throw new UnixIOException(result);
            }
        }
        
        public static void ChangeUserPassword(string password)
        {

        }

        public static void AddGroup(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new ArgumentNullException("Group cannot be empty");
            }
            Process proc = new Process();
            proc.StartInfo.FileName = "addgroup";
            proc.StartInfo.Arguments = group;
            proc.Start();
            proc.WaitForExit();

            string result = proc.StandardOutput.ReadToEnd();
            if (!result.Contains("Created"))
            {
                throw new UnixIOException(result);
            }
        }

        public static void AddUserToGroup(string username, string group)
        {
            if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("Username / Group cannot be empty");
            }
            Process proc = new Process();
            proc.StartInfo.FileName = "adduser";
            proc.StartInfo.Arguments = string.Format("{0} {1}", username, group);
            string result = proc.StandardOutput.ReadToEnd();
            if (!result.Contains("Added"))
            {
                throw new UnixIOException(result);
            }
        }

        public static void GetUserList (string username)
        {

        }

        public static void GetGroupList (string group)
        {

        }
    }
}
