﻿using System;
using System.Runtime.Serialization;

namespace OperationLibrary.PacketProcessor
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException() : base()
        {
        }

        public InvalidTokenException(string message) : base(message)
        {
        }

        public InvalidTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidTokenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class Token : IDisposable
    {
        protected byte[] NToken;
        protected bool Disposed = false;
        protected const int ValidTokenLength = 95;

        public Token(byte[] token)
        {
            NToken = token;
        }

        public byte[] GetByte()
        {
            return NToken;
        }

        public string GetString()
        {
            try
            {
                string str = Convert.ToBase64String(NToken);
                return str;
            }
            catch (Exception e)
            {
                Utilities.ConsoleUtils.Print("Token.GetString(): " + e.Message);
                return string.Empty;
            }
        }

        public int Length
        {
            get
            {
                return NToken.Length;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool dispose)
        {
            if (!Disposed)
            {
                if (dispose)
                {
                    if (NToken != null)
                    {
                        NToken = null;
                    }
                }
                Disposed = true;
            }
        }

        ~Token()
        {
            Dispose(true);
        }
    }
}
