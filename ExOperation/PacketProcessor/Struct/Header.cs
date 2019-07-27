using ExOperation.Utilities;

namespace ExOperation.PacketProcessor
{
    public class Header
    {
        // For anti climate headering
        protected readonly byte[] ValidSignature = new byte[] { 0x81, 0xE9, 0x73, 0x8A };

        private byte[] Signature;

        public Header()
        {
            Signature = ValidSignature;
        }

        public Header(byte[] header) : this(header, 0)
        {
        }

        public Header(byte[] header, int offset)
        {
            Signature = ByteReader.ReadBytes(header, offset, 4);
        }

        public byte[] GetByte()
        {
            return Signature;
        }

        public bool IsValidHeader()
        {
            for (int index = 0; index < 4; index++)
            {
                if (Signature[index] != ValidSignature[index])
                    return false;
            }
            return true;
        }

        public int Length
        {
            get
            {
                return Signature.Length;
            }
        }
    }
}
