using ExOperation.PacketProcessor;
using System;
using System.IO;

namespace ExOperation.PacketParser
{
    public static class PacketParser
    {
        public static void ProcessCommand(Packet packet)
        {
            CommandEnum command = packet.Command;
            switch (command)
            {
                case CommandEnum.Login:
                    throw new NotImplementedException();
                case CommandEnum.Password:
                    throw new NotImplementedException();
                case CommandEnum.RequestToken:
                    throw new NotImplementedException();
                case CommandEnum.ActivateProject:
                    throw new NotImplementedException();
                case CommandEnum.DeactivateProject:
                    throw new NotImplementedException();
                case CommandEnum.CreateProject:
                    throw new NotImplementedException();
                case CommandEnum.DeleteProject:
                    throw new NotImplementedException();
                case CommandEnum.CreateDirectory:
                    throw new NotImplementedException();
                case CommandEnum.RenameDirectory:
                    throw new NotImplementedException();
                case CommandEnum.DeleteDirectory:
                    throw new NotImplementedException();
                case CommandEnum.CreateUser:
                    throw new NotImplementedException();
                case CommandEnum.ChangePassword:
                    throw new NotImplementedException();
                case CommandEnum.DeleteUser:
                    throw new NotImplementedException();
                case CommandEnum.CreateGroup:
                    throw new NotImplementedException();
                case CommandEnum.UserToGroup:
                    throw new NotImplementedException();
                case CommandEnum.UserFromGroup:
                    throw new NotImplementedException();
                case CommandEnum.DeleteGroup:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        public static Token GetToken()
        {
            byte[] tokenByte = new byte[95];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(tokenByte);
            return new Token(tokenByte);
        }

        public static Header GetSpecifiedHeader(byte[] parsing, int offset)
        {
            Header Header = new Header(parsing, offset);
            return Header;
        }

        public static Packet GetPacketFromBuffer(byte[] receivedbuffer, Stream stream)
        {
            return new Packet(receivedbuffer, stream);
        }

        public static Packet CreatePacket(ResponseEnum response)
        {
            return new Packet(response, new Token(new byte[95]));
        }

        public static Packet CreatePacketWithToken(ResponseEnum response, Token token)
        {
            return new Packet(response, token);
        }
    }
}
