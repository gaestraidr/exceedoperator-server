using Mono.Unix.Native;
using System;

namespace ExOperation.IOExecutor
{
    public static class DirectoryFileHelper
    {
        public static void CreateDirectory(string path, FilePermissions permission)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path shouldn't be null");
            }
            int status;
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
                throw new ArgumentException("Path shouldn't be null");
            }
            int status;
            status = Stdlib.rename(path, newPath);
            if (status != 0)
            {
                throw new Mono.Unix.UnixIOException((Errno)status);
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path shouldn't be null");
            }
            int status;
            status = Syscall.rmdir(path);
            if (status != 0)
            {
                throw new Mono.Unix.UnixIOException((Errno)status);
            }
        }
    }
}
