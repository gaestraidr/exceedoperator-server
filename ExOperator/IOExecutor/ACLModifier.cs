﻿using System;
using System.Diagnostics;

namespace OperationLibrary.IOExecutor
{
    public static class ACLModifier
    {
        // For those of you confused on why I love using System.Diagnostics.Process so much
        // Is because the Unix.Native.FilePermission didn't give us all the freedom to change the acl
        // And I don't think that Unix.Native.Syscall chmod/chown/chgrp accepting uint is a good idea
        // As I don't want any of UID parsed to the application memory even if it not used.
        internal static void ChangePermission(string path, uint aclcode, bool recursive)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("Path shouldn't be null");
            }
            string recur = recursive ? "-R " : string.Empty;
            Process proc = new Process();
            proc.StartInfo.FileName = "/bin/chmod";
            proc.StartInfo.Arguments = string.Format("{0}{1} {2}", recur, aclcode.ToString(), path);
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit();

            string result = proc.StandardOutput.ReadToEnd();
            if (!string.IsNullOrEmpty(result))
            {
                throw new Mono.Unix.UnixIOException("Change Permission: " + result);
            }
        }

        internal static void ChangeOwner(string path, string username, string group, bool recursive)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("Path shouldn't be null");
            }
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("Username cannot be null");
            }
            string recur = recursive ? "-R " : string.Empty;
            string formOwn = string.Format("{0}{1}", username, string.IsNullOrEmpty(group) ? string.Empty : ":" + group);
            Process proc = new Process();
            proc.StartInfo.FileName = "/bin/chown";
            proc.StartInfo.Arguments = string.Format("{0}{1} {2}", recur, formOwn, path);
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit();

            string result = proc.StandardOutput.ReadToEnd();
            if (!string.IsNullOrEmpty(result))
            {
                throw new Mono.Unix.UnixIOException("Change Owner: " + result);
            }
        }

        internal static void ChangeGroup(string path, string group, bool recursive)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("Path shouldn't be null");
            }
            if (string.IsNullOrEmpty(group))
            {
                throw new ArgumentNullException("Group cannot be null");
            }
            string recur = recursive ? "-R " : string.Empty;
            Process proc = new Process();
            proc.StartInfo.FileName = "/bin/chgrp";
            proc.StartInfo.Arguments = string.Format("{0}{1} {2}", recur, group, path);
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.WaitForExit();

            string result = proc.StandardOutput.ReadToEnd();
            if (!string.IsNullOrEmpty(result))
            {
                throw new Mono.Unix.UnixIOException("Change Group: " + result);
            }
        }
    }
}
