using OperationLibrary.PacketProcessor;
using OperationLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OperatorServer
{
    class Program
    {
        static List<Thread> elc = new List<Thread>();
        static List<TcpClient> epf = new List<TcpClient>();
        static TcpClient lastHandler = new TcpClient();
        static string lastEndPointAddr = "";

        static object stateObject = new object();

        static void Main(string[] args)
        {
            #region Unhandled Exception
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            #endregion

            Console.Title = "Operator Console - Server Side";
            TcpListener brs = new TcpListener(System.Net.IPAddress.Parse("0.0.0.0"), 1887);
            Console.WriteLine("[   Operator Auth - Server Container v1.00   ]");
            Console.WriteLine("| Creator:     GNS      ||      Ganesha      |");
            Console.WriteLine("==============================================");
            Console.WriteLine(""); Console.WriteLine("");
            Console.WriteLine("================================================================================");

            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                Console.WriteLine("");
                Console.WriteLine("The server can only run in Unix OS Environtment!");
                Console.WriteLine("Current OS: {0}", Environment.OSVersion.Platform.ToString());
                Console.WriteLine("");
                Console.Write("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            ConsoleUtils.Print("--- Begin setup component ---");

            if (!Directory.Exists("/project"))
            {
                ConsoleUtils.Print("Can't find folder root path, please create one!");
                Console.WriteLine(""); 
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            SetupFolder();

            try
            {
                using (var mysql = new OperationLibrary.DatabaseStorage.MySQLDatabaseConnection(IPAddress.Parse("127.0.0.1"), "exceeddb"))
                {
                    mysql.Username = "exceed";
                    mysql.Password = "xenodrom";
                    mysql.Connect();
                    mysql.Close();
                }
            }
            catch (Exception e)
            {
                ConsoleUtils.Print("Can't contact the database server, reason: " + e.Message);
                Console.WriteLine("");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            brs.Start();

            ConsoleUtils.Print("Done Setup!");
            ConsoleUtils.Print("");
            Thread.Sleep(1000);
            ConsoleUtils.Print("Searching for compatible socket to listen...");
            
            do
            {
                Thread.Sleep(50);
                if (brs.Pending())
                {
                    lastHandler = brs.AcceptTcpClient();
                    string currentSocketEndpoint = lastHandler.Client.LocalEndPoint.ToString();
                    lastEndPointAddr = lastHandler.Client.RemoteEndPoint.ToString();
                    Thread trd = new Thread(SocketThread);
                    trd.Start();
                    elc.Add(trd); epf.Add(lastHandler);
                    ConsoleUtils.Print(@"A new socket is now online (" + currentSocketEndpoint + ")");
                }

                Thread.Sleep(50);
                lock (stateObject) {
                    foreach (Thread trd in elc.ToArray())
                    {
                        if (!trd.IsAlive)
                            elc.Remove(trd);
                    }
                    foreach (TcpClient tcpCon in epf.ToArray())
                    {
                        if (!tcpCon.Connected)
                        {
                            try { tcpCon.Close(); } catch { }
                            epf.Remove(tcpCon);
                        }
                    }
                }
            }
            while (true);
        }

        private static void SocketThread()
        {
            string endPointTarget = lastEndPointAddr;
            try
            {
                TcpClient etl = null;
                lock (stateObject)
                {
                    etl = lastHandler;
                }

                // Sanity check
                if (etl == null)
                    throw new NullReferenceException("Failed to gain access to underlying socket of specified connection.");

                ConsoleUtils.Print("Connected socket (" + endPointTarget + "), began reading data from this instance...");

                using (NetworkStream nve = etl.GetStream())
                {
                    etl.ReceiveTimeout = 20000;
                    while (true)
                    {
                        nve.Flush();
                        Thread.Sleep(50);

                        byte[] recBytes = new byte[etl.ReceiveBufferSize];

                        int totalNumberRead = 0;

                        while (totalNumberRead < 229)
                        {
                            int numberRead = nve.Read(recBytes, totalNumberRead, ( recBytes.Length - totalNumberRead));
                            if (numberRead != 0)
                                totalNumberRead += numberRead;
                            else break;
                        }
                        Packet recPacket = PacketParser.GetPacketFromBuffer(recBytes, nve, endPointTarget);
                        if (recPacket.Header.IsValidHeader())
                        {
                            PacketParser.ProcessCommand(recPacket);
                        }
                        else
                        {
                            Packet throwPacket = PacketParser.CreatePacket(ResponseEnum.ReceiveFailed);
                            nve.Write(throwPacket.GetBytes(), 0, throwPacket.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e is ObjectDisposedException || e is IOException)
                {
                    ConsoleUtils.Print("[" + endPointTarget + "]'s Disconnected: " + e.Message);
                    return;
                }
                else
                {
                    ConsoleUtils.Print("Terminate Socket of [" + endPointTarget + "]'s cause of error: " + e.Message);
                    ConsoleUtils.Print("Searching for another socket to replace...");
                    return;
                }
            }
        }

        protected static void SetupFolder()
        {
            if (!Directory.Exists("/project/Trash"))
                Directory.CreateDirectory("/project/Trash");

            if (!Directory.Exists("/tmp/exceedop"))
                Directory.CreateDirectory("/tmp/exceedop");
        }

        #region Unhandled Exception - Method
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject != null)
            {
                Exception ex = (Exception)e.ExceptionObject;
                HandleUnhandledException(ex);
            }
        }

        private static void HandleUnhandledException(Exception ex)
        {
            string message = String.Format("Exception: {0}: {1} Source: {2} {3}", ex.GetType(), ex.Message, ex.Source, ex.StackTrace);
            ConsoleUtils.Print("[ERROR] " + message);
        }
        #endregion
    }
}
