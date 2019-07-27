using System.ComponentModel;
using System.IO;
using System;
using OperationLibrary.PacketProcessor;

namespace OperationLibrary.IOExecutor
{
    public class FileStoreHelper
    {
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected const int BufferSize = 20000000; // About 20 Megabytes/second... is this good? Nah let's just see

        public static void UploadFile(string path, Stream stream)
        {
            Packet sendPacket = PacketParser.CreatePacket(ResponseEnum.Confirm);
            stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
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
                        echleon.Write(bufferFile, TotalReadBytes, bufferFile.Length);
                        TotalReadBytes += CurReadBytes;
                    }
                }
                while (CurReadBytes != 0);
            }
        }

        public static void CopyFile(string path, string destPath)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(destPath))
            {
                throw new ArgumentNullException("Path/DestPath shouldn't be null");
            }

            System.IO.File.Copy(path, destPath);
        }

        public static void DownloadFile(string path, Stream stream)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File did not exist in the project");
            }

            byte[] recTemp = new byte[BufferSize];
            stream.Read(recTemp, 0, BufferSize);
            Packet tempPack = PacketParser.GetPacketFromBuffer(recTemp, null, null);
            if (tempPack.Command == CommandEnum.DownloadFile)
            {
                using (var limeshiet = new FileStream(path, FileMode.Open, FileAccess.Read))
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
            }
            else throw new InvalidAsynchronousStateException("User did not responded properly");
        }
    }
}
