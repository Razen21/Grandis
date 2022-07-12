using Grandis.Wz.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace Grandis.Wz.Utility
{
    public class WzReader : BinaryReader
    {

        private readonly WzEncryption _encrytpion;

        public WzReader(Stream input, WzEncryption wzEncryption) : base(input)
        {
            _encrytpion = wzEncryption;
        }

        public WzReader(Stream input, long offset, WzEncryption wzEncryption) : base(input)
        {
            _encrytpion = wzEncryption;
            BaseStream.Position = offset;
        }

        public WzReader Clone() => new WzReader(BaseStream, _encrytpion);

        public bool CanRead(long amount)
        {
            return (BaseStream.Position + amount) <= BaseStream.Length;
        }

        public void Skip(long distance)
            => BaseStream.Position += distance;

        public void Peek(Action action)
        {
            var origin = BaseStream.Position;

            try
            {
                action();
            }
            finally
            {
                BaseStream.Position = origin;
            }
        }



        public T Peek<T>(Func<T> action)
        {
            var origin = BaseStream.Position;

            try
            {
                return action();
            }
            finally
            {
                BaseStream.Position = origin;
            }
        }

        public short ReadWzShort()
        {
            var b = ReadSByte();
            return b == -128
                ? ReadInt16()
                : b;
        }

        public int ReadWzInt()
        {
            var b = ReadSByte();
            return b == -128
                ? ReadInt32()
                : b;
        }


        public long ReadWzLong()
        {
            var b = ReadSByte();
            return b == -128
                ? ReadInt64()
                : b;
        }

        public string ReadWZString(bool encrypted = true)
        {
            if (!CanRead(1))
            {
                throw new Exception("End of Stream");

            }
            int length = ReadSByte();
            if (length == 0)
            {
                return "";
            }
            if (length > 0)
            {
                length = length == 127 ? ReadInt32WithinBounds() : length;
                if (length == 0)
                {
                    return "";
                }
                if (BaseStream.Position + length * 2 > BaseStream.Length)
                {
                    throw new Exception("End of Stream");
                }
                byte[] rbytes = ReadBytes(length * 2);
                return _encrytpion.DecryptUnicodeString(rbytes, encrypted);
            }
            length = length == -128 ? ReadInt32WithinBounds() : -length;
            if (BaseStream.Position + length > BaseStream.Length)
            {
                throw new Exception("End of Stream");
            }
            return length == 0 ? "" : _encrytpion.DecryptASCIIString(ReadBytes(length), encrypted);
        }
        public string ReadWzStringBlock(bool encrypted)
        {
            switch (ReadByte())
            {
                case 0:
                case 0x73:
                    return ReadWZString(encrypted);
                case 1:
                case 0x1B:
                    return ReadWzStringAtOffset(ReadInt32WithinBounds(), encrypted);
                default:
                    return string.Empty;
                    
            }
        }

        private string ReadWzStringAtOffset(long offset, bool encrypted = true)
        {
            return Peek(() => {
                BaseStream.Position = offset;
                return ReadWZString(encrypted);
            });
        }

        private int ReadInt32WithinBounds()
        {
            if (BaseStream.Position + 4 > BaseStream.Length)
            {
                throw new Exception("End of Stream");
            }
            return ReadInt32();
        }

        public uint ReadOffset(uint start)
        {
            uint offset = (uint)BaseStream.Position;
            offset = (offset - start) ^ uint.MaxValue;
            offset *= 53940; // TODO Detect version
            offset -= WzEncryption.OffsetKey;
            offset = BitOperations.RotateLeft(offset, (byte)(offset & 0x1F));

            // Encrypted Offset
            uint encryptedOffset = ReadUInt32();
            offset ^= encryptedOffset;
            offset += start * 2;
            return offset;
        }


        
    }
}

