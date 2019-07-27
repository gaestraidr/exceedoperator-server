using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OperationLibrary.PacketProcessor
{
    public enum ResponseEnum
    {
        Success = 0x7D90,
        Failure = 0x583E,
        Confirm = 0x84C2,
        InvalidToken = 0x69FF,
        ReceiveOK = 0x70FA,
        ReceiveFailed = 0x0000
    }
}
