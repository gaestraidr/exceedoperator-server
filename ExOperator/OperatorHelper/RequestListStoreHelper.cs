using OperationLibrary.DatabaseStorage;
using OperationLibrary.PacketProcessor;
using OperationLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace OperationLibrary.OperatorHelper
{
    public class RequestListStoreHelper
    {
        protected static object stateProj = new object();
        protected static object stateGroup = new object();
        protected static object stateUser = new object();

        internal static void RequestProjectList(Packet packet)
        {
            string filepath = string.Format("/tmp/exceedop/projectlist_{0}", 
                DateTime.Now.ToString().Split(char.Parse(" "))[0].Replace(char.Parse("/"), char.Parse("_")));

            FileMode option = !File.Exists(filepath) ? FileMode.OpenOrCreate : FileMode.Truncate;

            lock (stateProj)
            {
                using (var fsl = new FileStream(filepath, option, FileAccess.Write))
                {
                    List<string[]> resDb;
                    using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                    {
                        dbCon.Username = "exceed";
                        dbCon.Password = "xenodrom";
                        dbCon.Connect();

                        string command = "SELECT * FROM `projectDB`";
                        resDb = dbCon.GetAllCollumnValue(command);
                        command = string.Empty;
                        dbCon.Close();
                    }

                    if (resDb.Count == 0)
                    {
                        Packet sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                        packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                        return;
                    }

                    StreamWriter writer = new StreamWriter(fsl);
                    writer.WriteLine("=== Project List ===");
                    writer.WriteLine("  ");
                    foreach (string[] list in resDb)
                    {
                        writer.WriteLine(string.Format("[{0}]", list[0]));
                        writer.WriteLine("   type: Avid Media Composer");
                        writer.WriteLine(string.Format("   groups: {0}", list[1]));
                        writer.WriteLine(string.Format("   users: {0}", list[2]));
                        writer.WriteLine("  ");
                    }
                    writer.Close();
                }
            }

            using (var file = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                Packet sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Confirm, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);

                int curReadBytes = 0;

                do
                {
                    byte[] sendByte = new byte[10000];
                    curReadBytes = file.Read(sendByte, 0, sendByte.Length);
                    if (curReadBytes != 0)
                    {
                        packet.Stream.Write(sendByte, 0, curReadBytes);
                        byte[] recByte = new byte[2];
                        packet.Stream.Read(recByte, 0, 2);
                        if (Utilities.ByteConverter.ToResponse(recByte, 0) != ResponseEnum.Confirm)
                        {
                            packet.Stream.Write(ByteReader.ReadBytes(ResponseEnum.Failure), 0, 2);
                            throw new InvalidAsynchronousStateException("User did not responded properly");
                        }
                    }
                    else
                    {
                        packet.Stream.Write(ByteReader.ReadBytes(ResponseEnum.Success), 0, 2);
                    }
                }
                while (curReadBytes != 0);
            }
        }

        internal static void RequestGroupList(Packet packet)
        {
            string filepath = string.Format("/tmp/exceedop/grouplist_{0}",
                    DateTime.Now.ToString().Split(char.Parse(" "))[0].Replace(char.Parse("/"), char.Parse("_")));

            FileMode option = !File.Exists(filepath) ? FileMode.OpenOrCreate : FileMode.Truncate;

            lock (stateGroup)
            {
                using (var fsl = new FileStream(filepath, option, FileAccess.Write))
                {
                    List<string[]> resDb;
                    using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                    {
                        dbCon.Username = "exceed";
                        dbCon.Password = "xenodrom";
                        dbCon.Connect();

                        string command = "SELECT * FROM `groupDB`";
                        resDb = dbCon.GetAllCollumnValue(command);
                        command = string.Empty;
                        dbCon.Close();
                    }

                    if (resDb.Count == 0)
                    {
                        Packet sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                        packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                        return;
                    }

                    StreamWriter writer = new StreamWriter(fsl);
                    writer.WriteLine("=== Group List ===");
                    writer.WriteLine("  ");
                    foreach (string[] list in resDb)
                    {
                        writer.WriteLine(string.Format("[{0}]", list[1]));
                        writer.WriteLine(string.Format("   dateCreated: {0}", list[2]));
                        writer.WriteLine(string.Format("   users: {0}", list[3]));
                        writer.WriteLine("  ");
                    }
                    writer.Close();
                }
            }

            using (var file = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                Packet sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Confirm, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);

                int curReadBytes = 0;

                do
                {
                    byte[] sendByte = new byte[10000];
                    curReadBytes = file.Read(sendByte, 0, sendByte.Length);
                    if (curReadBytes != 0)
                    {
                        packet.Stream.Write(sendByte, 0, curReadBytes);
                        byte[] recByte = new byte[2];
                        packet.Stream.Read(recByte, 0, 2);
                        if (Utilities.ByteConverter.ToResponse(recByte, 0) != ResponseEnum.Confirm)
                        {
                            packet.Stream.Write(ByteReader.ReadBytes(ResponseEnum.Failure), 0, 2);
                            throw new InvalidAsynchronousStateException("User did not responded properly");
                        }
                    }
                    else
                    {
                        packet.Stream.Write(ByteReader.ReadBytes(ResponseEnum.Success), 0, 2);
                    }
                }
                while (curReadBytes != 0);
            }
        }

        internal static void RequestUserList(Packet packet)
        {
            string filepath = string.Format("/tmp/exceedop/userlist_{0}",
                     DateTime.Now.ToString().Split(char.Parse(" "))[0].Replace(char.Parse("/"), char.Parse("_")));

            FileMode option = !File.Exists(filepath) ? FileMode.OpenOrCreate : FileMode.Truncate;

            lock (stateUser)
            {
                using (var fsl = new FileStream(filepath, option, FileAccess.Write))
                {
                    List<string[]> resDb;
                    using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                    {
                        dbCon.Username = "exceed";
                        dbCon.Password = "xenodrom";
                        dbCon.Connect();

                        string command = "SELECT `username`, `name`, `dateCreated` FROM `userDB`";
                        resDb = dbCon.GetAllCollumnValue(command);
                        command = string.Empty;
                        dbCon.Close();
                    }

                    if (resDb.Count == 0)
                    {
                        Packet sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Success, packet.Token);
                        packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
                        return;
                    }

                    StreamWriter writer = new StreamWriter(fsl);
                    writer.WriteLine("=== User List ===");
                    writer.WriteLine("  ");
                    foreach (string[] list in resDb)
                    {
                        writer.WriteLine(string.Format("[{0}]", list[0]));
                        writer.WriteLine(string.Format("   fullname: {0}", list[1]));
                        writer.WriteLine(string.Format("   dateCreated: {0}", list[2]));
                        writer.WriteLine("  ");
                    }
                    writer.Close();
                }
            }

            using (var file = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                Packet sendPacket = PacketParser.CreatePacketWithToken(ResponseEnum.Confirm, packet.Token);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);

                int curReadBytes = 0;

                do
                {
                    byte[] sendByte = new byte[10000];
                    curReadBytes = file.Read(sendByte, 0, sendByte.Length);
                    if (curReadBytes != 0)
                    {
                        packet.Stream.Write(sendByte, 0, curReadBytes);
                        byte[] recByte = new byte[2];
                        packet.Stream.Read(recByte, 0, 2);
                        if (Utilities.ByteConverter.ToResponse(recByte, 0) != ResponseEnum.Confirm)
                        {
                            packet.Stream.Write(ByteReader.ReadBytes(ResponseEnum.Failure), 0, 2);
                            throw new InvalidAsynchronousStateException("User did not responded properly");
                        }
                    }
                    else
                    {
                        packet.Stream.Write(ByteReader.ReadBytes(ResponseEnum.Success), 0, 2);
                    }
                }
                while (curReadBytes != 0);
            }
        }
    }
}
