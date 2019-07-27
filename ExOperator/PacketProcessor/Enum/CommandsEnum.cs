using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OperationLibrary.PacketProcessor
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
        MemberToProject = 0x99D0,
        MemberFromProject = 0x00F5,
        CreateDirectory = 0x33E9,
        RenameDirectory = 0x8E9D,
        DeleteDirectory = 0x77FE,
        CreateProject = 0x9F0A,
        DeleteProject = 0x10F4,
        ActivateProject = 0xF72B,
        DeactivateProject = 0x8EDB,
        RequestUserList = 0x9B1C,
        RequestGroupList = 0x0E8F,
        RequestProjectList = 0xEAFF,
        UploadFile = 0xEFFE,
        DownloadFile = 0xFEEF,
        KeepAlive = 0x00F9,
        UnspecifiedCommand = 0x0000
    }
}
