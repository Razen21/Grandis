using Grandis.Wz.Utility;
using Grandis.Wz.Utility.Types;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grandis.Wz.Objects
{
    public class WzFile
    {

        public string Name { get; private set; }
        public string FilePath { get; private set; }
        public WzVariant Variant { get; private set; }
        public WzHeader Header { get; private set; }
        public WzDirectory MainDirectory { get; private set; }

        private WzReader _reader;
        private MemoryMappedFile _mmf;
        private WzEncryption _encryption;
        

        private WzFile(string path)
        {
            Name = Path.GetFileName(path);
            FilePath = path;
            //Variant = WzVariant.AUTO;
            _mmf = MemoryMappedFile.CreateFromFile(FilePath, FileMode.Open);
            _encryption = new WzEncryption(WzVariant.GMS);
            _reader = new WzReader(_mmf.CreateViewStream(), _encryption);

            ParseFile();
        }

        private WzFile(string path, WzVariant variant)
        {
            Name = Path.GetFileName(path);
            FilePath = path;
            Variant = variant;
            _mmf = MemoryMappedFile.CreateFromFile(FilePath, FileMode.Open);
            _encryption = new WzEncryption(variant);
            _reader = new WzReader(_mmf.CreateViewStream(), _encryption);

            ParseFile();
        }

        /// <summary>
        /// Static factory method for creating a new instance of a WzFile from an existing path.
        /// </summary>
        /// <param name="path">Location of the WzFile</param>
        /// <returns>New instance of a WzFile with the specified path</returns>
        public static WzFile CreateFromPath(string path)
        {
            return new WzFile(path);
        }

        /// <summary>
        /// Static factory method for creating a new instance of a WzFile from an existing path with a WzVariant.
        /// </summary>
        /// <param name="path">Location of the WzFile</param>
        /// <param name="variant">WzVariant Enum of the type of WzFile</param>
        /// <returns>New instance of a WzFile with the specified path</returns>
        public static WzFile CreateFromPath(string path, WzVariant variant)
        {
            return new WzFile(path, variant);
        }


        private void ParseFile()
        {
            /*
            if (Variant == WzVariant.AUTO)
            {
                Variant = TryGetWzVariant(out WzVariant variant) 
                    ? variant 
                    : WzVariant.UNDEFINED;
            }

            if (Variant == WzVariant.UNDEFINED)
            {
                Console.WriteLine("Cannot parse a file with an undefined WzVariant. Please REPORT!");
            }
            */

            
            ConstructHeader();
            ResolveDirectories();
        }

        private void ConstructHeader()
        {

            // Local method for reading the Wizet Package Signature of .wz Files 
            bool ReadSignature(out string signature)
            {
                signature = new string(_reader.ReadChars(4));

                return signature == "PKG1";
            }

            // 1: Read The Entire size of the Memory Stream
            long fileSize = _reader.BaseStream.Length;

            // 2: Read the Signature (first 4 bytes). Return the signature if approved.
            if (!ReadSignature(out string signature))
            {
                Console.WriteLine("Signature did not match required PKG1");
                return;
            }

            // 3: The size of the file after the Signature
            long dataSize = _reader.ReadInt64();

            // 4: The entire size of the Header (Signature + Copyright) + Optional Version bytes.
            int headerSize = _reader.ReadInt32();

            // 5: Read the Copyright with a string lenght of (Header Size - Current Stream Position)
            //    Typical Copyright: Package file v1.0 Copyright 2002 Wizet, ZMS
            string copyright = new string(_reader.ReadChars(headerSize - (int)_reader.BaseStream.Position));

            // 6: Starting position in the stream for the actual data.
            long startPosition = _reader.BaseStream.Position;

            // 7: Encode Version consistent accross all files.
            short encodeVersion = _reader.ReadInt16();

            Console.WriteLine(encodeVersion);

            // 7: Instantiate the WzHeader for this WzFile Instance
            Header = new WzHeader()
            {
                Signature = signature,
                Copyright = copyright,
                HeaderSize = headerSize,
                DataSize = dataSize,
                FileSize = fileSize,
                StartPosition = startPosition,
                EncodeVersion = encodeVersion
            };


        }

        private void ResolveDirectories()
        {
            MainDirectory = new WzDirectory("", null!, this, _reader, _reader.BaseStream.Position);
        }

        private bool TryGetWzVariant(out WzVariant variant)
        {
            variant = WzVariant.GMS;
            return true;
        }

        /// <summary>
        /// Create a substream of the MemoryMappedFile and jump to the defined offset.
        /// </summary>
        /// <param name="offset">The position to set the new BaseStream</param>
        /// <returns>New instance of WzReader</returns>
        public WzReader GetSubStream(long offset)
        {
            return new WzReader(_mmf.CreateViewStream(), offset, _encryption);
        }

    }

    public struct WzHeader
    {
        public string? Signature { get; set; }
        public string? Copyright { get; set; }
        public int HeaderSize { get; set; }
        public long DataSize { get; set; }
        public long FileSize { get; set; }
        public long StartPosition { get; set; }
        public short EncodeVersion { get; set; }
    }
}
