using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExOperation.PacketProcessor
{
    public enum CommandEnum
    {
        Login = 0x4D6E,
        Password = 0x32CE,
        RequestToken = 0xE2E9,
        CreateUser = 0x23E7,
        DeleteUser = 0x283E,
        ChangePassword = 0x87E2,
        CreateGroup = 0xFE9D,
        DeleteGroup = 0x7FCD,
        UserToGroup = 0x8FE3,
        UserFromGroup = 0x83DF,
        CreateDirectory = 0x33E9,
        RenameDirectory = 0x8E9D,
        DeleteDirectory = 0x77FE,
        CreateProject = 0x9F0A,
        DeleteProject = 0x10F4,
        ActivateProject = 0xF72B,
        DeactivateProject = 0x8EDB,
        UnspecifiedCommand = 0x0000
    }
}
