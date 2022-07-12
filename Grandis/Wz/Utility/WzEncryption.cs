using Grandis.Wz.Utility.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Grandis.Wz.Utility
{
    public class WzEncryption
    {

        private byte[] _asciiEncKey;
        private byte[] _asciiKey;
        private byte[] _unicodeEncKey;
        private byte[] _unicodeKey;
        private byte[] _wzKey;

        private readonly WzVariant _version;

        public WzEncryption(WzVariant version)
        {
            _version = version;
            GenerateKeys(ushort.MaxValue);
        }

        private void GenerateKeys(int length)
        {
            _wzKey = GetWZKey(_version, length);
            byte[] asciiKey = new byte[_wzKey.Length];
            byte[] unicodeKey = new byte[_wzKey.Length];
            byte[] asciiEncKey = new byte[_wzKey.Length];
            byte[] unicodeEncKey = new byte[_wzKey.Length];
            unchecked
            {
                byte mask = 0xAA;
                for (int i = 0; i < _wzKey.Length; ++i, ++mask)
                {
                    asciiKey[i] = mask;
                    asciiEncKey[i] = (byte)(_wzKey[i] ^ mask);
                }
                ushort umask = 0xAAAA;
                for (int i = 0; i < _wzKey.Length / 2; i += 2, ++umask)
                {
                    unicodeKey[i] = (byte)(umask & 0xFF);
                    unicodeKey[i + 1] = (byte)((umask & 0xFF00) >> 8);
                    unicodeEncKey[i] = (byte)(_wzKey[i] ^ unicodeKey[i]);
                    unicodeEncKey[i + 1] = (byte)(_wzKey[i + 1] ^ unicodeKey[i + 1]);
                }
            }
            _asciiKey = asciiKey;
            _unicodeKey = unicodeKey;
            _asciiEncKey = asciiEncKey;
            _unicodeEncKey = unicodeEncKey;
        }

        internal string DecryptASCIIString(byte[] asciiBytes, bool encrypted = true)
        {
            CheckKeyLength(asciiBytes.Length);
            return Encoding.ASCII.GetString(DecryptData(asciiBytes, encrypted ? _asciiEncKey : _asciiKey));
        }

        internal string DecryptUnicodeString(byte[] ushortChars, bool encrypted = true)
        {
            CheckKeyLength(ushortChars.Length);
            return Encoding.Unicode.GetString(DecryptData(ushortChars, encrypted ? _unicodeEncKey : _unicodeKey));
        }

        internal byte[] DecryptBytes(byte[] bytes)
        {
            CheckKeyLength(bytes.Length);
            return DecryptData(bytes, _wzKey);
        }

        private void CheckKeyLength(int length)
        {
            if (_wzKey.Length < length)
            {
                GenerateKeys(length);
            }
        }

        internal const uint OffsetKey = 0x581C3F6D;

        private static readonly byte[] AESKey = {
            0x13, 0x00, 0x00, 0x00,
            0x08, 0x00, 0x00, 0x00,
            0x06, 0x00, 0x00, 0x00,
            0xB4, 0x00, 0x00, 0x00,
            0x1B, 0x00, 0x00, 0x00,
            0x0F, 0x00, 0x00, 0x00,
            0x33, 0x00, 0x00, 0x00,
            0x52, 0x00, 0x00, 0x00
        };

        private static readonly byte[] GMSIV = {
            0x4D, 0x23, 0xC7, 0x2B,
            0x4D, 0x23, 0xC7, 0x2B,
            0x4D, 0x23, 0xC7, 0x2B,
            0x4D, 0x23, 0xC7, 0x2B
        };

        private static readonly byte[] KMSIV = {
            0xB9, 0x7D, 0x63, 0xE9,
            0xB9, 0x7D, 0x63, 0xE9,
            0xB9, 0x7D, 0x63, 0xE9,
            0xB9, 0x7D, 0x63, 0xE9
        };

        private static byte[] GetWZKey(WzVariant version, int length)
        {
            length = (length & ~15) + ((length & 15) > 0 ? 16 : 0);
            switch ((int)version)
            {
                case 0:
                    return GenerateKey(KMSIV, AESKey, length);
                case 1:
                    return GenerateKey(GMSIV, AESKey, length);
                case 2:
                    return new byte[length];
                default:
                    throw new ArgumentException("Invalid WZ variant passed.", nameof(version));
            }
        }

        private static byte[] GenerateKey(byte[] iv, byte[] aesKey, int length)
        {
            using (MemoryStream memStream = new MemoryStream(length))
            {
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.Key = aesKey;
                    aes.Mode = CipherMode.CBC;
                    aes.IV = iv;

                    using (
                        CryptoStream cStream = new CryptoStream(memStream, aes.CreateEncryptor(), CryptoStreamMode.Write)
                        )
                    {
                        cStream.Write(new byte[length], 0, length);
                        cStream.Flush();
                        return memStream.ToArray();
                    }
                }
            }
        }

        private static unsafe byte[] DecryptData(byte[] data, byte[] key)
        {
            if (data.Length > key.Length)
            {
                throw new InvalidOperationException("data.Length > key.Length; not supposed to happen, please report this to reWZ");
            }

            fixed (byte* c = data, k = key)
            {
                byte* d = c, l = k, e = d + data.Length;
                while (d < e)
                {
                    *d++ ^= *l++;
                }
            }

            return data;
        }

        public static uint RotateLeft(uint x, byte n)
        {
            return (uint)(((x) << (n)) | ((x) >> (32 - (n))));
        }
    }


}


