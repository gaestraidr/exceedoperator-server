using ExOperation.PacketProcessor;
using System;

namespace ExOperation.Utilities
{
    public class ByteReader
    {
        public static byte ReadByte(byte[] bytes, int offset)
        {
            return bytes[offset];
        }

        public static byte ReadByte(char character)
        {
            if (character <= char.MaxValue)
                return Convert.ToByte(character);
            else throw new ArgumentException("Inputed character should not reaching Maximum value assigned");
        }

        public static byte ReadByteFromInt16(short targetinterger)
        {
            if (targetinterger <= short.MaxValue || targetinterger >= short.MinValue)
                return Convert.ToByte(targetinterger);
            else throw new ArgumentException("Inputed Int16 should not reaching Minimum/Maximum value");
        }

        public static byte ReadByteFromUInt16(ushort targetinterger)
        {
            if (targetinterger <= ushort.MaxValue || targetinterger >= ushort.MinValue)
                return Convert.ToByte(targetinterger);
            else throw new ArgumentException("Inputed Unsigned Int16 should not reaching Minimum/Maximum value");
        }

        public static byte ReadByteFromInt32(int targetinterger)
        {
            if (targetinterger <= int.MaxValue || targetinterger >= int.MinValue)
                return Convert.ToByte(targetinterger);
            else throw new ArgumentException("Inputed Int32 should not reaching Minimum/Maximum value");
        }

        public static byte ReadByteFromUInt32(uint targetinterger)
        {
            if (targetinterger <= uint.MaxValue || targetinterger >= uint.MinValue)
                return Convert.ToByte(targetinterger);
            else throw new ArgumentException("Inputed Unsigned Int32 should not reaching Minimum/Maximum value");
        }

        public static byte ReadByteFromInt64(long targetinterger)
        {
            if (targetinterger <= long.MaxValue || targetinterger >= long.MinValue)
                return Convert.ToByte(targetinterger);
            else throw new ArgumentException("Inputed Int64 should not reaching Minimum/Maximum value");
        }

        public static byte ReadByteFromUInt64(ulong targetinterger)
        {
            if (targetinterger <= ulong.MaxValue || targetinterger >= ulong.MinValue)
                return Convert.ToByte(targetinterger);
            else throw new ArgumentException("Inputed Int16 should not reaching Minimum/Maximum value");
        }


        public static byte[] ReadBytes(CommandEnum command)
        {
            byte[] retBytes = new byte[2];
            ushort comCon = (ushort)command;
            retBytes[0] = (byte)comCon;
            retBytes[1] = (byte)(comCon >> 8);
            return retBytes;
        }

        public static byte[] ReadBytes(ResponseEnum response)
        {
            byte[] retBytes = new byte[2];
            ushort comCon = (ushort)response;
            retBytes[0] = (byte)comCon;
            retBytes[1] = (byte)(comCon >> 8);
            return retBytes;
        }


        public static byte[] ReadBytes(byte[] bytes, int offset, int count)
        {
            byte[] newByte = new byte[count];
            try { Array.Copy(bytes, offset, newByte, 0, count); } catch { }
            return newByte;
        }

        public static byte[] ReadBytes(string targetstring)
        {
            byte[] newByte = new byte[targetstring.Length];
            try { newByte = Encoding.UTF8.GetBytes(targetstring); } catch { }
            return newByte;
        }
    }
}