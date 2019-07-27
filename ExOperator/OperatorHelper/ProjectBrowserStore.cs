using OperationLibrary.IOExecutor;
using System.IO;
using System;
using System.Collections.Generic;

namespace OperationLibrary.OperatorHelper
{
    public class ProjectBrowserStore
    {
        protected const string _mainPath = "/project/";

        internal static ProjectCollection GetListProject()
        {
            return new ProjectCollection();
        }

        internal static string[] GetTreePathinProject(Project project, string path)
        {
            if (project == null)
                throw new ArgumentNullException("Can't find inputed project.");

            string combine = project.FullPath + path;
            string[] havocDir = Directory.GetDirectories(combine, "*", SearchOption.TopDirectoryOnly);
            string[] havocFiles = Directory.GetFiles(combine, "*", SearchOption.TopDirectoryOnly);
            string[] havoc = new string[havocDir.Length + havocFiles.Length];
            havocDir.CopyTo(havoc, 0); havocFiles.CopyTo(havoc, havocDir.Length);
            for (int i = 0; i < havoc.Length; i++)
            {
                if (Directory.Exists(havoc[i]))
                {
                    havoc[i] += " [DIR]";
                }
                havoc[i] = havoc[i].Remove(0, combine.Length);
            }
            return havoc;
        }

        internal static void UpdateAppendedProject()
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "/usr/local/samba/sbin/smbd";
            proc.StartInfo.Arguments = "reload";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            StreamReader strB = proc.StandardOutput;
            string result = strB.ReadToEnd();
            proc.WaitForExit();
            proc.Close();
        }

        internal static Project CreateProject(string projectName)
        {
            string mainDir = _mainPath + projectName.Replace(Char.Parse(" "), Char.Parse("_"));
            if (!Directory.Exists(mainDir))
            {
                SetupFolder(mainDir, projectName);
            }
            else throw new InvalidOperationException("Project already exist"); 

            Project project = new Project(projectName);
            using (var setting = new FileStream(@"/etc/smb.conf", FileMode.Append, FileAccess.Write, FileShare.Read, 0x1000, FileOptions.WriteThrough))
            {
                StreamWriter best = new StreamWriter(setting);
                best.WriteLine(string.Format("#[{0}]", project.ProjectName));
                best.WriteLine(string.Format("#  comment = Project of {0}", project.ProjectName));
                best.WriteLine(string.Format("#  path = {0}", project.FullPath));
                best.WriteLine("#  valid users = admin");
                best.WriteLine("#  vfs objects = media_harmony");
                best.WriteLine("#  hide unreadable = yes");
                best.WriteLine("#  veto oplock files = /*.lock/");
                best.WriteLine("#  read only = no");
                best.WriteLine("#  browseable = yes");
                best.WriteLine("#  guest ok = no");
                best.WriteLine("#  create mask = 3777");
                best.WriteLine("#  directory mask = 3755");
                best.WriteLine("#  invalid users = guest");
                best.WriteLine("  ");
                best.Flush();
                best.Close();
            }
            return project;
        }

        internal static void DeleteProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException("Can't find inputed project.");

            const string config = @"/etc/smb.conf";
            int LineNumber = -1;
            using (var setting = new FileStream(config, FileMode.Open, FileAccess.Read))
            {
                int count = 0;
                StreamReader best = new StreamReader(setting);
                do
                {
                    string getIn = best.ReadLine();
                    if (getIn == string.Format(@"#[{0}]", project.ProjectName))
                    {
                        LineNumber = count;
                        break;
                    }
                    else if (getIn == string.Format(@"[{0}]", project.ProjectName))
                        throw new MemberAccessException("The project is not deactivated.");
                    else count++;
                }
                while (!best.EndOfStream);
                best.Close();
                setting.Close();
            }
            if (LineNumber == -1)
                throw new EntryPointNotFoundException("Couldn't find the project name in the config.");

            List<string> append = new List<string>(File.ReadAllLines(config));
            for (int i = 0; i <= 11; i++)
            {
                append.RemoveAt(LineNumber);
            }
            File.WriteAllLines(config, append.ToArray());
            UpdateAppendedProject();
            DirectoryFileHelper.RenameDirectory(project.FullPath, project.FullPath.Insert(9, "Trash/"));
        }

        internal static void ActivateProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException("Can't find inputed project.");

            const string config = @"/etc/smb.conf";
            int LineNumber = -1;
            using (var setting = new FileStream( config, FileMode.Open, FileAccess.Read))
            {
                int count = 0;
                StreamReader best = new StreamReader(setting);
                do
                {
                    string getIn = best.ReadLine();
                    if (getIn == string.Format(@"#[{0}]", project.ProjectName))
                    {
                        LineNumber = count;
                        break;
                    }
                    else if (getIn == string.Format("[{0}]", project.ProjectName))
                    {
                        throw new InvalidOperationException("Project is already activated.");
                    }
                    else count++;
                }
                while (!best.EndOfStream);
                best.Close();
                setting.Close();
            }
            if (LineNumber == -1)
                throw new EntryPointNotFoundException("Couldn't find the project name in the config.");

            string[] append = File.ReadAllLines(config);
            for ( int i = LineNumber; i <= (LineNumber + 11); i++)
            {
                append[i] = append[i].Remove(0, 1);
            }
            File.WriteAllLines(config, append);
            UpdateAppendedProject();
        }
        
        internal static void DeactivateProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException("project", "Can't find inputed project.");

            const string config = @"/etc/smb.conf";
            int LineNumber = -1;
            using (var setting = new FileStream(config, FileMode.Open, FileAccess.Read))
            {
                int count = 0;
                StreamReader best = new StreamReader(setting);
                do
                {
                    string getIn = best.ReadLine();
                    if (getIn == string.Format("[{0}]", project.ProjectName))
                    {
                        LineNumber = count;
                        break;
                    }
                    else if (getIn == string.Format("#[{0}]", project.ProjectName))
                    {
                        throw new InvalidOperationException("Project is already deactivated.");
                    }
                    else count++;
                }
                while (!best.EndOfStream);
                best.Close();
                setting.Close();
            }
            if (LineNumber == -1)
                throw new EntryPointNotFoundException("Couldn't find the project name in the config.");

            string[] append = File.ReadAllLines(config);
            for (int i = LineNumber; i <= (LineNumber + 11); i++)
            {
                append[i] = @"#" + append[i];
            }
            File.WriteAllLines(config, append);
            UpdateAppendedProject();
        }

        internal static void AddMemberToProject(string member, Project project)
        {
            if (project == null)
                throw new ArgumentNullException("Can't find inputed project.");

            const string config = @"/etc/smb.conf";
            int LineNumber = -1;

            using (var setting = new FileStream(config, FileMode.Open, FileAccess.Read))
            {
                int count = 0;
                StreamReader best = new StreamReader(setting);
                do
                {
                    string getIn = best.ReadLine();
                    if (getIn == string.Format("[{0}]", project.ProjectName) ||
                        getIn == string.Format("#[{0}]", project.ProjectName))
                    {
                        LineNumber = count;
                        break;
                    }
                    else count++;
                }
                while (!best.EndOfStream);
                best.Close();
                setting.Close();
            }

            if (LineNumber == -1)
                throw new EntryPointNotFoundException("Couldn't find the project name in the config.");
            else LineNumber += 3;

            string[] append = File.ReadAllLines(config);
            string account = null;

            if (member.Contains("[U]"))
            {
                account = member.Substring(4);
            }
            else
            {
                string grp = member.Substring(4);
                int gid = UserManagement.UserInfoInserter.CheckForGroupGID(grp);
                if (gid != -1)
                    account = "@GID" + gid.ToString();
                else throw new MissingMemberException(string.Format("Group {0} is not exist in the server", grp));
            }

            int lengthLine = append[LineNumber].Contains("#") ? 17 : 16;

            if (append[LineNumber].Length <= lengthLine)
                append[LineNumber] += account;
            else append[LineNumber] += string.Format(", {0}", account);

            File.WriteAllLines(config, append);
            UpdateAppendedProject();
        }

        internal static void RemoveMemberFromProject(string member, Project project)
        {
            if (project == null)
                throw new ArgumentNullException("Can't find inputed project.");

            const string config = @"/etc/smb.conf";
            int LineNumber = -1;

            using (var setting = new FileStream(config, FileMode.Open, FileAccess.Read))
            {
                int count = 0;
                StreamReader best = new StreamReader(setting);
                do
                {
                    string getIn = best.ReadLine();
                    if (getIn == string.Format("[{0}]", project.ProjectName) ||
                        getIn == string.Format("#[{0}]", project.ProjectName))
                    {
                        LineNumber = count;
                        break;
                    }
                    else count++;
                }
                while (!best.EndOfStream);
                best.Close();
                setting.Close();
            }

            if (LineNumber == -1)
                throw new EntryPointNotFoundException("Couldn't find the project name in the config.");
            else LineNumber += 3;

            string account = null;
            string[] append = File.ReadAllLines(config);
            if (member.Contains("[U]"))
            {
                account = member.Substring(4);
            }
            else
            {
                string grp = member.Substring(4);
                int gid = UserManagement.UserInfoInserter.CheckForGroupGID(grp);
                if (gid != -1)
                    account = "@GID" + gid.ToString();
                else throw new MissingMemberException(string.Format("Group {0} is not exist in the server", grp));
            }

            int lengthLine = append[LineNumber].Contains("#") ? 17 : 16;

            int index = append[LineNumber].IndexOf(account);

            if (index != -1)
            {
                int count = account.Length + 2;
                if ((index + count) > (append[LineNumber].Length - 1) && index != lengthLine)
                    append[LineNumber] = append[LineNumber].Remove((index - 2), count);
                else try { append[LineNumber] = append[LineNumber].Remove(index, count); }
                    catch { append[LineNumber] = append[LineNumber].Remove(index, (count - 2)); }
            }
            else
            {
                throw new MissingMemberException(string.Format("Member ({0}) is not found in the project {1}.", member, project.ProjectName));
            }

            File.WriteAllLines(config, append);
            UpdateAppendedProject();
        }

        protected static void SetupFolder(string path, string projectname)
        {
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(string.Format("{0}/{1}", path, projectname));
            Directory.CreateDirectory(string.Format("{0}/{1}/Other Users", path, projectname));
            Directory.CreateDirectory(string.Format("{0}/Avid MediaFiles/MXF/1", path));
            FileStoreHelper.CopyFile("/project/.Template_Avid/tmpl_Proj.avp", string.Format("{0}/{1}/{2}.avp", path, projectname, projectname));
            FileStoreHelper.CopyFile("/project/.Template_Avid/tmpl_Proj Settings.avs", string.Format("{0}/{1}/{2} Settings.avs", path, projectname, projectname));
            FileStoreHelper.CopyFile("/project/.Template_Avid/tmpl_Proj Settings.xml", string.Format("{0}/{1}/{2} Settings.xml", path, projectname, projectname));
            Directory.CreateDirectory(string.Format("{0}/{1}/SearchData", path, projectname));
            Directory.CreateDirectory(string.Format("{0}/{1}/Statistics", path, projectname));
            Directory.CreateDirectory(string.Format("{0}/{1}/WaveformCache", path, projectname));
            Directory.CreateDirectory(string.Format("{0}/{1}/Trash", path, projectname));

            ACLModifier.ChangePermission(path, 3777, true);
        }
    }
}
