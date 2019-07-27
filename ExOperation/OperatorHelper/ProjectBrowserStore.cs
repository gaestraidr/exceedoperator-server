using ExOperation.IOExecutor;
using System.IO;
using System;

namespace ExOperation.OperatorHelper
{
    public class ProjectBrowserStore
    {
        protected const string _mainPath = "/project/";

        public static ProjectCollection GetListProject()
        {
            return new ProjectCollection();
        }

        public static string[] GetTreePathinProject(Project project, string path)
        {
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
            }
            return havoc;
        }

        public static void UpdateAppendedProject()
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "smbd";
            proc.StartInfo.Arguments = "reload";
            proc.Start();
            proc.WaitForExit();

            string result = proc.StandardOutput.ReadToEnd();
            if (!string.IsNullOrEmpty(result))
            {
                throw new Mono.Unix.UnixIOException("Updating Project: " + result);
            }
        }

        public static Project CreateProject(string projectName)
        {
            if (!Directory.Exists(_mainPath + projectName))
                Directory.CreateDirectory(_mainPath + projectName);
            return new Project(projectName);
        }

        public static void DeleteProject(Project project)
        {
            DirectoryFileHelper.DeleteDirectory(project.FullPath);
        }

        public static void ActivateProject(Project project)
        {
            using (var setting = new FileStream(@"/usr/local/samba/etc/smb.conf", FileMode.Append, FileAccess.Write, FileShare.Read, 0x1000, FileOptions.WriteThrough))
            {
                StreamWriter best = new StreamWriter(setting);
                best.WriteLine(string.Format("[{0}]", project.ProjectName));
                best.WriteLine(string.Format("  comment = Project of {0}", project.ProjectName));
                best.WriteLine(string.Format("  path = {0}"), project.FullPath);
                best.WriteLine("  vfs objects = media_harmony");
                best.WriteLine("  hide unreadable = yes");
                best.WriteLine("  veto oplock files = /*.lock/");
                best.WriteLine("  read only = no");
                best.WriteLine("  browseable = yes");
                best.WriteLine("  guest ok = no");
                best.WriteLine("  create mask = 3750");
                best.WriteLine("  directory mask = 3750");
                best.WriteLine("  ");
                best.Flush();
                best.Close();
            }
            UpdateAppendedProject();
        }
    }
}
