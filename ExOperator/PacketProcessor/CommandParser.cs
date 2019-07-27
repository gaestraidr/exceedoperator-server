using OperationLibrary.UserManagement;
using OperationLibrary.DatabaseStorage;
using OperationLibrary.Utilities;
using OperationLibrary.OperatorHelper;
using System;
using OperationLibrary.IOExecutor;
using System.IO;
using OperationLibrary.Cleaner;

namespace OperationLibrary.PacketProcessor
{
    public class CommandParser
    {
        /*
         * In this class, there were many derived method that should be simpler, but no
         * as this is open the freedom to process more command if any appended change is
         * bound to set, and we will group it within this container, so a memory leak won't be a problem
         */

        public static void GenerateToken(Packet packet)
        {
            Packet sendPacket;
            try
            {
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Requesting token exchange from server [{1}]", packet.ClientIP, packet.Username));
                Token newToken = PacketParser.GenerateToken();
                using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                {
                    dbCon.Username = "exceed";
                    dbCon.Password = "xenodrom";
                    dbCon.Connect();
                    string brt = dbCon.GetSqlDateTimeNow(2);
                    string command = string.Format("UPDATE `adminDB` SET `token`='{0}', `tokenExpired`='{1}' WHERE `username`='{2}'", newToken.GetString(), 
                        brt, packet.Username);
                    dbCon.ExecuteCommand(command);
                    sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.ReceiveOK, newToken);
                    command = string.Empty;
                    dbCon.Close();
                }
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Generate Token Failure: {0}:{1}", e.Source, e.Message));
                sendPacket = PacketParser.CreatePacket(ResponseEnum.Failure);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void VerifyUser(Packet packet)
        {
            using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
            {
                Packet sendPacket;
                dbCon.Username = "exceed";
                dbCon.Password = "xenodrom";
                dbCon.Connect();
                string command = string.Format("SELECT `username` FROM `adminDB` WHERE `username`='{0}'", packet.Username);
                bool ver = dbCon.SearchForBoolean(command, null);

                if (ver)
                {
                    ConsoleUtils.Print(string.Format("[{0}]'s Request: Logging in to the server [{1}]", packet.ClientIP, packet.Username));
                    sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Confirm, packet.Token);
                }
                else
                {
                    ConsoleUtils.Print(string.Format("[{0}]'s Denied: Logging in with wrong username [{1}]", packet.ClientIP, packet.Username));
                    sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                }
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                command = string.Empty;
            }
        }

        public static void VerifyPassword(Packet packet)
        {
            using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
            {
                Packet sendPacket;
                dbCon.Username = "exceed";
                dbCon.Password = "xenodrom";
                dbCon.Connect();
                string command = string.Format("SELECT `username`, `password` FROM `adminDB` WHERE `username`='{0}'", packet.Username);
                string passInDB = dbCon.SearchForString(command, "password");
                
                bool foo = CryptSharp.Crypter.CheckPassword(packet.Argument, passInDB);

                if (foo)
                {
                    ConsoleUtils.Print(string.Format("[{0}]'s Allowed: Logging in with valid credential [{1}]", packet.ClientIP, packet.Username));
                    sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                }
                else
                {
                    ConsoleUtils.Print(string.Format("[{0}]'s Denied: Logging in with wrong password [{1}]", packet.ClientIP, packet.Username));
                    sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                }
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                command = string.Empty;
            }
        }

        public static void CreateUser(Packet packet)
        {
            Packet sendPacket;
            try
            {
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Create new user to the server [{1}]", packet.ClientIP, packet.Username));
                string[] store = packet.Argument.Split(char.Parse("|"));
                string name = store[0];
                string username = store[1];
                if (username.ToLower().Contains("root") || username == Environment.UserName)
                {
                    ConsoleUtils.Print(string.Format("Adding User Error: User requesting to create unallowed name [{0}]", username));
                    sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                    packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                    return;
                }
                string password = store[2];
                UserInfoInserter.AddUser(username, password);
                using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                {
                    dbCon.Username = "exceed";
                    dbCon.Password = "xenodrom";
                    dbCon.Connect();
                    
                    string command = string.Format("INSERT INTO `exceeddb`.`userDB` (`name`, `username`, `password`, `dateCreated`," +
                        " `token`, `tokenExpired`) VALUES ('{0}', '{1}', '{2}', '{3}', '', '')", name, username, CryptSharp.Crypter.Sha512.Crypt(password), DateTime.Now.ToString());
                    dbCon.ExecuteCommand(command);
                    command = string.Empty;
                    dbCon.Close();
                }
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Success: Successfully creating user {1} [{2}]", packet.ClientIP, username,  packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Adding User Error: {0}", e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void ChangePassword(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string[] store = packet.Argument.Split(char.Parse("|"));
                string username = store[0];
                string password = store[1];
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Changing password of user [{1}] in the server [{2}]", packet.ClientIP, username, packet.Username));
                if (username == "root" || username == Environment.UserName)
                {
                    ConsoleUtils.Print(string.Format("Change Password Error: User requesting to change password of unallowed name [{0}]", username));
                    sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                    packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                    return;
                }
                UserInfoInserter.ChangeUserPassword(username, password);
                using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                {
                    dbCon.Username = "exceed";
                    dbCon.Password = "xenodrom";
                    dbCon.Connect();

                    string command = string.Format("UPDATE `userDB` SET `password`='{0}' WHERE `username`='{1}'", CryptSharp.Crypter.Sha512.Crypt(password), username);
                    dbCon.ExecuteCommand(command);
                    command = string.Empty;
                    dbCon.Close();
                }
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Change Password Error: {0}", e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void DeleteUser(Packet packet)
        {
            Packet sendPacket;
            try
            {
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Delete user to the server [{1}]", packet.ClientIP, packet.Username));
                string username = packet.Argument;
                if (username == "root" || username == Environment.UserName)
                {
                    ConsoleUtils.Print(string.Format("[{0}]'s Denied: User requesting to remove unallowed name: {1} [{2}]", packet.ClientIP, username, packet.Username));
                    sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                    packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                    return;
                }
                
                UserInfoInserter.DeleteUser(username);
                using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                {
                    dbCon.Username = "exceed";
                    dbCon.Password = "xenodrom";
                    dbCon.Connect();

                    string command = string.Format("DELETE FROM `userDB` WHERE (`username`= '{0}')", username);
                    dbCon.ExecuteCommand(command);
                    command = string.Empty;
                    dbCon.Close();
                }
                UserCleaner.RemoveUserInEveryGroup(username);
                UserCleaner.RemoveUserInEveryProject(username);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Successfully deleting user {1} [{2}]", packet.ClientIP, username, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Delete User Error: {0}", e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void CreateProject(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string[] split = packet.Argument.Split(char.Parse("|"));
                string projectName = split[0];
                string activate = split[1];
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Create new project to the server [{1}]", packet.ClientIP, packet.Username));
                Project newProj = ProjectBrowserStore.CreateProject(projectName);
                ProjectBrowserStore.ActivateProject(newProj);

                using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                {
                    dbCon.Username = "exceed";
                    dbCon.Password = "xenodrom";
                    dbCon.Connect();

                    string command = string.Format("INSERT INTO `exceeddb`.`projectDB` (`projectname`, `group`, `user`)" +
                        " VALUES ('{0}', '', '')", newProj.ProjectName);
                    dbCon.ExecuteCommand(command);
                    command = string.Empty;
                    dbCon.Close();
                }

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Create Project Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }
        
        public static void DeleteProject(Packet packet)
        {
            Packet sendPacket;
            try
            {
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Delete project to the server [{1}]", packet.ClientIP, packet.Username));
                Project project = null;
                ProjectCollection projCol = ProjectBrowserStore.GetListProject();
                foreach (Project proj in projCol)
                {
                    if (proj.ProjectName == packet.Argument)
                    {
                        project = proj;
                        break;
                    }
                }

                ProjectBrowserStore.DeactivateProject(project);
                ProjectBrowserStore.DeleteProject(project);

                using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                {
                    dbCon.Username = "exceed";
                    dbCon.Password = "xenodrom";
                    dbCon.Connect();

                    string command = string.Format("DELETE FROM `projectDB` WHERE (`projectname`= '{0}')", project.ProjectName);
                    dbCon.ExecuteCommand(command);
                    command = string.Empty;
                    dbCon.Close();
                }

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Successfully deleting project: {1} [{2}]", packet.ClientIP, project.ProjectName, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Delete Project Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void ActivateProject(Packet packet)
        {
            Packet sendPacket;
            try
            {
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Activating project in the server [{1}]", packet.ClientIP, packet.Username));
                Project project = null;
                ProjectCollection projCol = ProjectBrowserStore.GetListProject();
                foreach (Project proj in projCol)
                {
                    if (proj.ProjectName == packet.Argument)
                    {
                        project = proj;
                        break;
                    }
                }

                ProjectBrowserStore.ActivateProject(project);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Activating project success: {1} [{2}]", packet.ClientIP, project.ProjectName, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Activate Project Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void DeactivateProject(Packet packet)
        {
            Packet sendPacket;
            try
            {
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Deactivating project in the server [{1}]", packet.ClientIP, packet.Username));
                Project project = null;
                ProjectCollection projCol = ProjectBrowserStore.GetListProject();
                foreach (Project proj in projCol)
                {
                    if (proj.ProjectName == packet.Argument)
                    {
                        project = proj;
                        break;
                    }
                }

                ProjectBrowserStore.DeactivateProject(project);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Deactivating project success: {1} [{2}]", packet.ClientIP, project.ProjectName, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Deactivate Project Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void UserToGroup(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string[] combine = packet.Argument.Split(char.Parse("|"));
                string username = combine[0];
                string group = combine[1];
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Add user to group {1} [{2}]", packet.ClientIP, group, packet.Username));

                UserInfoInserter.AddUserToGroup(username, group);
                MySQLAccountParsing.AddUserToGroup(username, group);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Adding user to group success: {1} [{2}]", packet.ClientIP, username, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("User To Group Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void UserFromGroup(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string[] combine = packet.Argument.Split(char.Parse("|"));
                string username = combine[0];
                string group = combine[1];
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Remove user from group {1} [{2}]", packet.ClientIP, group, packet.Username));

                UserInfoInserter.RemoveUserFromGroup(username, group);
                MySQLAccountParsing.RemoveUserFromGroup(username, group);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Removing user from group success: {1} [{2}]", packet.ClientIP, username, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("User From Group Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void MemberToProject(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string[] combine = packet.Argument.Split(char.Parse("|"));
                string member = combine[0];
                string projectname = combine[1];
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Adding member to project {1} [{2}]",
                    packet.ClientIP, projectname, packet.Username));

                Project project = null;
                ProjectCollection projCol = ProjectBrowserStore.GetListProject();
                foreach (Project proj in projCol)
                {
                    if (proj.ProjectName == projectname)
                    {
                        project = proj;
                        break;
                    }
                }

                ProjectBrowserStore.AddMemberToProject(member, project);
                MySQLAccountParsing.AddMemberToProject(member, project);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Adding member to project success: {1} [{2}]", packet.ClientIP, member, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Member To Group Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }

        }

        public static void MemberFromProject(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string[] combine = packet.Argument.Split(char.Parse("|"));
                string member = combine[0];
                string projectname = combine[1];
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Remove member from project {1} [{2}]", 
                    packet.ClientIP, projectname, packet.Username));

                Project project = null;
                ProjectCollection projCol = ProjectBrowserStore.GetListProject();
                foreach (Project proj in projCol)
                {
                    if (proj.ProjectName == projectname)
                    {
                        project = proj;
                        break;
                    }
                }

                ProjectBrowserStore.RemoveMemberFromProject(member, project);
                MySQLAccountParsing.RemoveMemberFromProject(member, project);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Removing member from project success: {1} [{2}]", packet.ClientIP, member, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Member From Project Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void CreateGroup(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string group = packet.Argument;
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Add group to the server [{2}]", packet.ClientIP, group, packet.Username));

                int gid = UserInfoInserter.AddGroup(group);

                using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                {
                    dbCon.Username = "exceed";
                    dbCon.Password = "xenodrom";
                    dbCon.Connect();

                    string command = string.Format("INSERT INTO `exceeddb`.`groupDB` (`groupid`, `groupname`, `dateCreated`, `member`)" +
                        " VALUES ('{0}', '{1}', '{2}', '')", gid, group, DateTime.Now.ToString());
                    dbCon.ExecuteCommand(command);
                    command = string.Empty;
                    dbCon.Close();
                }

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Adding group success: {1} [{2}]", packet.ClientIP, group, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Adding Group Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void RemoveGroup(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string group = packet.Argument;
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Remove group from the server [{2}]", packet.ClientIP, group, packet.Username));

                UserInfoInserter.RemoveGroup(group);

                using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                {
                    dbCon.Username = "exceed";
                    dbCon.Password = "xenodrom";
                    dbCon.Connect();

                    string command = string.Format("DELETE FROM `groupDB` WHERE (`groupname`= '{0}')", group);
                    dbCon.ExecuteCommand(command);
                    command = string.Empty;
                    dbCon.Close();
                }

                GroupCleaner.RemoveGroupFromEveryProject(group);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Removing group success: {1} [{2}]", packet.ClientIP, group, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Remove Group Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void CreateDirectory(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string[] splited = packet.Argument.Split(char.Parse("|"));
                Project project = new Project(splited[0]);
                string path = splited[1];
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Create directory to project {1} [{2}]", packet.ClientIP, project.ProjectName, packet.Username));

                Directory.CreateDirectory(project.FullPath + path);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Creating directory success: {1} [{2}]", packet.ClientIP, path.Substring(path.LastIndexOf(char.Parse("/"))), packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Create Directory Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void RenameDirectory(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string[] splited = packet.Argument.Split(char.Parse("|"));
                Project project = new Project(splited[0]);
                string path = splited[1];
                string newPath = splited[2];
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Rename directory in project {1} [{2}]", packet.ClientIP, project.ProjectName, packet.Username));

                DirectoryFileHelper.RenameDirectory(project.FullPath + path, project.FullPath + newPath);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format(@"[{0}]'s Request: Rename directory success: {1} >> {2} [{3}]", packet.ClientIP, path.Substring(path.LastIndexOf(char.Parse("/"))),
                    newPath.Substring(path.LastIndexOf(char.Parse("/"))), packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Rename Directory Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void DeleteDirectory(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string[] splited = packet.Argument.Split(char.Parse("|"));
                Project project = new Project(splited[0]);
                string path = splited[1];
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Delete directory to project {1} [{2}]", packet.ClientIP, project.ProjectName, packet.Username));

                DirectoryFileHelper.DeleteDirectory(project.FullPath + path);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Deleting directory success: {1} [{2}]", packet.ClientIP, path.Substring(path.LastIndexOf(char.Parse("/"))), packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Delete Directory Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void UploadFile(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string[] splited = packet.Argument.Split(char.Parse("|"));
                Project project = new Project(splited[0]);
                string path = splited[1];
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Upload file to project {1} [{2}]", packet.ClientIP, project.ProjectName, packet.Username));

                FileStoreHelper.UploadFile(project.FullPath + path, packet.Stream);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Upload file success: {1} [{2}]", packet.ClientIP, path.Substring(path.LastIndexOf(char.Parse("/"))), packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Upload File Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            }
        }

        public static void DownloadFile(Packet packet)
        {
            Packet sendPacket;
            try
            {
                string[] splited = packet.Argument.Split(char.Parse("|"));
                Project project = new Project(splited[0]);
                string path = splited[1];
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Download file from project {1} [{2}]", packet.ClientIP, project.ProjectName, packet.Username));

                FileStoreHelper.DownloadFile(project.FullPath + path, packet.Stream);

                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Download file success: {1} [{2}]", packet.ClientIP, path.Substring(path.LastIndexOf(char.Parse("/"))), packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Download File Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                return;
            } 
        }

        public static void RequestProjectList(Packet packet)
        {
            Packet sendPacket;
            try
            {
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Requesting project list [{1}]", packet.ClientIP, packet.Username));

                RequestListStoreHelper.RequestProjectList(packet);

                ConsoleUtils.Print(string.Format("[{0}]'s Request: Successfully transfer project list [{1}]", packet.ClientIP, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Request Project List Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                if (!(e is System.ComponentModel.InvalidAsynchronousStateException))
                {
                    sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                    packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                }
                return;
            }
        }

        public static void RequestGroupList(Packet packet)
        {
            Packet sendPacket;
            try
            {
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Requesting group list [{1}]", packet.ClientIP, packet.Username));

                RequestListStoreHelper.RequestGroupList(packet);

                ConsoleUtils.Print(string.Format("[{0}]'s Request: Successfully transfer group list [{1}]", packet.ClientIP, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Request Group List Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                if (!(e is System.ComponentModel.InvalidAsynchronousStateException))
                {
                    sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                    packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                }
                return;
            }
        }

        public static void RequestUserList(Packet packet)
        {
            Packet sendPacket;
            try
            {
                ConsoleUtils.Print(string.Format("[{0}]'s Request: Requesting user list [{1}]", packet.ClientIP, packet.Username));

                RequestListStoreHelper.RequestUserList(packet);

                ConsoleUtils.Print(string.Format("[{0}]'s Request: Successfully transfer user list [{1}]", packet.ClientIP, packet.Username));
            }
            catch (Exception e)
            {
                ConsoleUtils.Print(string.Format("Request User List Error: [{0}] {1}", e.GetType().ToString(), e.Message));
                if (!(e is System.ComponentModel.InvalidAsynchronousStateException))
                {
                    sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
                    packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                }
                return;
            }
        }

        public static void UnspecifiedCommand(Packet packet)
        {
            ConsoleUtils.Print(string.Format("[{0}]'s Request: Request received can't be identified [{1}]", packet.ClientIP, packet.Username));
            Packet sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Failure, packet.Token);
            packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
            return;
        }
    }
}
