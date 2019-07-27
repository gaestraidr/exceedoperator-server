using System.IO;

namespace ExOperation.IOExecutor
{
    public class FileStoreHelper
    {
        protected const int BufferSize = 20000000; // About 20 Megabytes/second... is this good? Nah let's just see

        public static void UploadFile(string path, Stream stream)
        {
            using ( var echleon = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                int TotalReadBytes = 0;
                int CurReadBytes = 0;
                do
                {
                    byte[] bufferFile = new byte[BufferSize];
                    CurReadBytes = stream.Read(bufferFile, 0, BufferSize);

                    if (CurReadBytes != 0)
                    {
                        echleon.Write(bufferFile, TotalReadBytes, BufferSize);
                        TotalReadBytes += CurReadBytes;
                    }
                }
                while (CurReadBytes != 0);
            }
        }

        public static int DownloadFile(string path, Stream stream)
        {
            int status = 0;
            if (!File.Exists(path))
            {
                status = -1;
                return status;
            }
            using ( var limeshiet = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                int TotalReadBytes = 0;
                int CurReadBytes = 0;
                do
                {
                    byte[] EtaTerangkanlah = new byte[BufferSize];
                    CurReadBytes = limeshiet.Read(EtaTerangkanlah, TotalReadBytes, BufferSize);

                    if (CurReadBytes != 0)
                    {
                        stream.Write(EtaTerangkanlah, 0, BufferSize);
                        TotalReadBytes += CurReadBytes;
                    }
                }
                while (CurReadBytes != 0);
            }
            return status;
        }
    }
}
