using System;
using System.IO;

namespace OperationLibrary.Utilities
{
    public class ConsoleUtils
    {
        protected static object state = new object();

        public static void Print(string printInput)
        {
            Print(printInput, true);
        }

        public static void Print(string printInput, bool writeline)
        {
            lock (state)
            {
                string hour = DateTime.Now.Hour.ToString().Length == 2 ? DateTime.Now.Hour.ToString() : "0" + DateTime.Now.Hour.ToString();
                string minute = DateTime.Now.Minute.ToString().Length == 2 ? DateTime.Now.Minute.ToString() : "0" + DateTime.Now.Minute.ToString();
                string second = DateTime.Now.Second.ToString().Length == 2 ? DateTime.Now.Second.ToString() : "0" + DateTime.Now.Second.ToString();
                string compact = string.Format("[{0}:{1}:{2}]: {3}", hour, minute, second, printInput);
                if (writeline)
                    Console.WriteLine(compact);
                else Console.Write(compact);

                if (!Directory.Exists(Environment.CurrentDirectory + "Logs"))
                    Directory.CreateDirectory("Logs");

                string filepath = string.Format(@"Logs/log-{0}_{1}_{2}.log", DateTime.Now.Day.ToString(),
                    DateTime.Now.Month.ToString(), DateTime.Now.Year.ToString());

                using (var openLog = new FileStream(filepath, FileMode.Append, FileAccess.Write, FileShare.Read, 0x1000, FileOptions.WriteThrough))
                {
                    StreamWriter writer = new StreamWriter(openLog);
                    writer.WriteLine(compact);
                    writer.Close();
                }
            }
        }
    }
}
