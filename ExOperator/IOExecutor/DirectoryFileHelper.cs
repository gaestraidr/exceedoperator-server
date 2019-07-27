using Mono.Unix.Native;
using System;
using System.Diagnostics;

namespace OperationLibrary.IOExecutor
{
    public static class DirectoryFileHelper
    {
        public static void CreateDirectory(string path, FilePermissions permission)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("Path shouldn't be null");
            }
            int status = 0;
            status = Syscall.mkdir(path, permission);
            if (status != 0)
            {
                throw new Mono.Unix.UnixIOException((Errno)status);
            }
        }

        public static void RenameDirectory(string path, string newPath)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("Path shouldn't be null");
            }

            string filterPath = newPath;
            int exist = 1;
            while (System.IO.Directory.Exists(filterPath)) {
                filterPath = string.Format("{0}-{1}", newPath, exist >= 10 ? exist.ToString() : "0" + exist.ToString());
                exist++;
            }

            System.IO.Directory.Move(path, filterPath);
        }

        public static void DeleteDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("Path shouldn't be null");
            }
            int status = 0;
            status = Syscall.rmdir(path);
            if (status != 0)
            {
                throw new Mono.Unix.UnixIOException((Errno)status);
            }
        }
    }
}
