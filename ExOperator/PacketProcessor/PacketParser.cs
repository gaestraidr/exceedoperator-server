using System;
using System.IO;

namespace OperationLibrary.PacketProcessor
{
    public static class PacketParser
    {
        public static void ProcessCommand(Packet packet)
        {
            CommandEnum command = packet.Command;

            if (command == CommandEnum.RequestToken)
            {
                CommandParser.GenerateToken(packet);
            }
            else if (command == CommandEnum.KeepAlive)
            {
                Packet sendPacket = CreatePacket(ResponseEnum.ReceiveOK);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
            }
            else if (packet.HasValidToken())
            {
                switch (command)
                {
                    case CommandEnum.Login:
                        CommandParser.VerifyUser(packet); break;
                    case CommandEnum.Password:
                        CommandParser.VerifyPassword(packet); break;
                    case CommandEnum.ActivateProject:
                        CommandParser.ActivateProject(packet); break;
                    case CommandEnum.DeactivateProject:
                        CommandParser.DeactivateProject(packet); break;
                    case CommandEnum.CreateProject:
                        CommandParser.CreateProject(packet); break;
                    case CommandEnum.DeleteProject:
                        CommandParser.DeleteProject(packet); break;
                    case CommandEnum.CreateDirectory:
                        CommandParser.CreateDirectory(packet); break;
                    case CommandEnum.RenameDirectory:
                        CommandParser.RenameDirectory(packet); break;
                    case CommandEnum.DeleteDirectory:
                        CommandParser.DeleteDirectory(packet); break;
                    case CommandEnum.CreateUser:
                        CommandParser.CreateUser(packet); break;
                    case CommandEnum.ChangePassword:
                        CommandParser.ChangePassword(packet); break;
                    case CommandEnum.DeleteUser:
                        CommandParser.DeleteUser(packet); break;
                    case CommandEnum.CreateGroup:
                        CommandParser.CreateGroup(packet); break;
                    case CommandEnum.UserToGroup:
                        CommandParser.UserToGroup(packet); break;
                    case CommandEnum.UserFromGroup:
                        CommandParser.UserFromGroup(packet); break;
                    case CommandEnum.MemberToProject:
                        CommandParser.MemberToProject(packet); break;
                    case CommandEnum.MemberFromProject:
                        CommandParser.MemberFromProject(packet); break;
                    case CommandEnum.DeleteGroup:
                       CommandParser.RemoveGroup(packet); break;
                    case CommandEnum.RequestProjectList:
                        CommandParser.RequestProjectList(packet); break;
                    case CommandEnum.RequestUserList:
                        CommandParser.RequestUserList(packet); break;
                    case CommandEnum.RequestGroupList:
                        CommandParser.RequestGroupList(packet); break;
                    default:
                        CommandParser.UnspecifiedCommand(packet); break;
                }
            }
            else
            {
                Utilities.ConsoleUtils.Print(string.Format("[{0}]'s Denied: The token given is not valid in the server [{1}]", packet.ClientIP, packet.Username));
                Packet sendPacket = CreatePacket(ResponseEnum.InvalidToken);
                packet.Stream.Write(sendPacket.GetBytes(), 0, sendPacket.Length);
            }
        }

        public static Token GenerateToken()
        {
            byte[] tokenByte = new byte[95];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = 
                new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(tokenByte);
            return new Token(tokenByte);
        }

        public static Header GetSpecifiedHeader(byte[] parsing, int offset)
        {
            Header Header = new Header(parsing, offset);
            return Header;
        }

        public static Packet GetPacketFromBuffer(byte[] receivedbuffer, Stream stream, string clientip)
        {
            return new Packet(receivedbuffer, stream, clientip);
        }

        public static Packet CreatePacket(ResponseEnum response, string argument = null)
        {
            return new Packet(response, new Token(new byte[95]), argument);
        }

        public static Packet CreatePacketWithToken(ResponseEnum response, Token token, string argument = null)
        {
            return new Packet(response, token, argument);
        }
    }
}
