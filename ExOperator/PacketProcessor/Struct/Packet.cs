using OperationLibrary.DatabaseStorage;
using OperationLibrary.PacketProcessor;
using OperationLibrary.Utilities;
using System;
using System.IO;

namespace OperationLibrary.PacketProcessor
{
    public class Packet
    {
        protected const int ConfidentialLength = 229;
        private bool Disposed = false;

        public string ClientIP;
        public Stream Stream;
        public Header Header;
        public Token Token;
        public CommandEnum Command;
        public ResponseEnum Response;
        public string Username;
        public string Argument;

        public Packet(byte[] targetParsing, Stream stream, string clientIP)
        {
            ClientIP = clientIP;
            Stream = stream;
            Header = new Header(targetParsing);
            Command = ByteConverter.ToCommand(targetParsing, 4);
            Token = new Token(ByteReader.ReadBytes(targetParsing, 6, 95));
            string[] Combined = ByteConverter.ToUTF8String(targetParsing, 101, 127).Trim('\0').Split(char.Parse(":"));
            Username = Combined[0];
            Argument = Combined[1];
        }

        public Packet(ResponseEnum response, Token token, string argument)
        {
            Header = new Header();
            Response = response;
            Token = token;
            if (string.IsNullOrEmpty(argument))
                Argument = "NoArgs";
            else Argument = argument;
        }

        public byte[] GetBytes()
        {
            byte[] conBytes = new byte[ConfidentialLength];
            Array.Copy(Header.GetByte(), 0, conBytes, 0, Header.Length);
            byte[] repByte = ByteReader.ReadBytes(Response);
            Array.Copy(repByte, 0, conBytes, 4, repByte.Length);
            Array.Copy(Token.GetByte(), 0, conBytes, 6, Token.Length);
            byte[] argsByte = ByteReader.ReadBytes(Argument);
            Array.Copy(argsByte, 0, conBytes, 101, argsByte.Length);
            return conBytes;
        }

        public int Length
        {
            get
            {
                return ConfidentialLength;
            }
        }

        public bool HasValidToken()
        {
            try
            {
                string convertedToken;
                string result;
                using (var dbCon = new MySQLDatabaseConnection(System.Net.IPAddress.Parse("127.0.0.1"), "exceeddb"))
                {
                    dbCon.Username = "exceed";
                    dbCon.Password = "xenodrom";
                    dbCon.Connect();

                    string command = string.Format("SELECT * FROM `adminDB` WHERE `username`='{0}'", Username);
                    string expToken = dbCon.SearchForString(command, "tokenExpired");
                    if (string.IsNullOrEmpty(expToken))
                        return false;
                   
                    DateTime validate = DateTime.Parse(expToken);
                    if (DateTime.Now > validate)
                        return false;

                    result = dbCon.SearchForString(command, "token");
                    convertedToken = Token.GetString();
                    command = string.Empty;

                    dbCon.Close();
                }
                return convertedToken == result;
            }
            catch (Exception e)
            {
                ConsoleUtils.Print("HasValidToken(): " + e.Message);
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool Dispose)
        {
            if (!Disposed)
            {
                if (Dispose)
                {
                    if (Token != null)
                    {
                        Token = null;
                    }
                    if (Header != null)
                    {
                        Header = null;
                    }
                    if (Command != 0)
                    {
                        Command = 0;
                    }
                    if (Response != 0)
                    {
                        Response = 0;
                    }
                    if (!string.IsNullOrEmpty(Username))
                    {
                        Username = null;
                    }
                    if (!string.IsNullOrEmpty(Argument))
                    {
                        Argument = null;
                    }
                }
                Disposed = true;
            }
        }

        ~Packet()
        {
            Dispose(true);
        }

    }

}
