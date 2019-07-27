using OperationLibrary.Utilities;
using System.ComponentModel;

namespace OperationLibrary.PacketProcessor
{
    public class Header
    {
        // For anti climate headering
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
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
            if (Signature.Length != ValidSignature.Length)
            {
                return false;
            }

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
