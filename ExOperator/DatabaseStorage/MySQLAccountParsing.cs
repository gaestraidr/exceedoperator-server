using OperationLibrary.OperatorHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OperationLibrary.DatabaseStorage
{
    public class MySQLAccountParsing
    {
        internal static void AddMemberToProject(string member, Project project)
        {
            using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
            {
                dbCon.Username = "exceed";
                dbCon.Password = "xenodrom";
                dbCon.Connect();

                string mode = member.Contains("[U]") ? "user" : "group";

                string command = string.Format("SELECT `{0}` FROM `projectDB` WHERE `projectname`='{1}'", mode, project.ProjectName);
                string projMember = dbCon.SearchForString(command, mode);
                string memberAcc = member.Substring(4);

                if (string.IsNullOrEmpty(projMember))
                    projMember = memberAcc;
                else projMember += string.Format(", {0}", memberAcc);

                command = string.Format("UPDATE `projectDB` SET `{0}`='{1}' WHERE `projectname`='{2}'", mode, projMember, project.ProjectName);
                dbCon.ExecuteCommand(command);
                command = string.Empty;
                dbCon.Close();
            }

        }
        internal static void RemoveMemberFromProject(string member, Project project)
        {
            using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
            {
                dbCon.Username = "exceed";
                dbCon.Password = "xenodrom";
                dbCon.Connect();

                string mode = member.Contains("[U]") ? "user" : "group";

                string command = string.Format("SELECT `{0}` FROM `projectDB` WHERE `projectname`='{1}'", mode, project.ProjectName);
                string projMember = dbCon.SearchForString(command, mode);
                string memberAcc = member.Substring(4);

                if (string.IsNullOrEmpty(projMember))
                    throw new MissingMemberException(string.Format("Member [{0}] not found in project {1}.", member, project.ProjectName));

                int index = projMember.IndexOf(memberAcc);

                if (index == -1)
                    throw new MissingMemberException(string.Format("Member [{0}] not found in project {1}.", member, project.ProjectName));

                int count = memberAcc.Length + 2;
                if ((index + count) > (projMember.Length - 1) && index != 0)
                    projMember = projMember.Remove((index - 2), count);
                else try { projMember = projMember.Remove(index, count); }
                    catch { projMember = projMember.Remove(index, (count - 2)); }

                command = string.Format("UPDATE `projectDB` SET `{0}`='{1}' WHERE `projectname`='{2}'", mode, projMember, project.ProjectName);
                dbCon.ExecuteCommand(command);
                command = string.Empty;
                dbCon.Close();
            }
        }

        internal static void AddUserToGroup(string username, string group)
        {
            using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
            {
                dbCon.Username = "exceed";
                dbCon.Password = "xenodrom";
                dbCon.Connect();

                string command = string.Format("SELECT `member` FROM `groupDB` WHERE `groupname`='{0}'", group);
                string grpMember = dbCon.SearchForString(command, "member");

                if (string.IsNullOrEmpty(grpMember))
                {
                    grpMember += username;
                }
                else grpMember += string.Format(", {0}", username);

                command = string.Format("UPDATE `groupDB` SET `member`='{0}' WHERE `groupname`='{1}'", grpMember, group);
                dbCon.ExecuteCommand(command);
                command = string.Empty;
                dbCon.Close();
            }
        }
        internal static void RemoveUserFromGroup(string username, string group)
        {
            using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
            {
                dbCon.Username = "exceed";
                dbCon.Password = "xenodrom";
                dbCon.Connect();

                string command = string.Format("SELECT `member` FROM `groupDB` WHERE `groupname`='{0}'", group);
                string grpMember = dbCon.SearchForString(command, "member");

                if (string.IsNullOrEmpty(grpMember))
                    throw new MissingMemberException(string.Format("Username {0} not found in group {1}.", username, group));

                int index = grpMember.IndexOf(username);

                if (index == -1)
                    throw new MissingMemberException(string.Format("Username {0} not found in group {1}.", username, group));

                int count = username.Length + 2;
                if ((index + count) > (grpMember.Length - 1) && index != 0)
                    grpMember = grpMember.Remove((index - 2), count);
                else try { grpMember = grpMember.Remove(index, count); } //To make sure the user name is in the first index or not
                    catch { grpMember = grpMember.Remove(index, (count - 2)); }

                command = string.Format("UPDATE `groupDB` SET `member`='{0}' WHERE `groupname`='{1}'", grpMember, group);
                dbCon.ExecuteCommand(command);
                command = string.Empty;
                dbCon.Close();
            }
        }
    }
}
