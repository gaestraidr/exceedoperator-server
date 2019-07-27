using ExOperation.PacketProcessor;
using System;
using System.Text;

namespace ExOperation.Utilities
{
    public class ByteConverter
    {
        public static CommandEnum ToCommand(byte[] targetBytes, int offset)
        {
            return (CommandEnum)((targetBytes[offset + 1] << 8) | (targetBytes[offset] << 0));
        }

        public static ResponseEnum ToResponse(byte[] targetBytes, int offset)
        {
            return (ResponseEnum)((targetBytes[offset + 1] << 8) | (targetBytes[offset] << 0));
        }

        public static string ToUTF8String(byte[] targetBytes)
        {
            return ToUTF8String(targetBytes, 0, targetBytes.Length);
        }

        public static string ToUTF8String(byte[] targetBytes, int offset, int length)
        {
            try
            {
                string retStr = Encoding.UTF8.GetString(targetBytes, offset, length);
                return retStr;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string ToASCIIString(byte[] targetBytes)
        {
            return ToASCIIString(targetBytes, 0, targetBytes.Length);
        }

        public static string ToASCIIString(byte[] targetBytes, int offset, int length)
        {
            try
            {
                string retStr = ASCIIEncoding.GetEncoding(28591).GetString(targetBytes, offset, length);
                return retStr;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
