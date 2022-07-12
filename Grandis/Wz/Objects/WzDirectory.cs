using Grandis.Wz.Utility;
using Grandis.Wz.Utility.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grandis.Wz.Objects
{
    public sealed class WzDirectory : WzObject
    {
        internal WzDirectory(string name, WzObject parent, WzFile parentFile, WzReader reader, long startPosition)
            : base(name, parent, parentFile, true, WzObjectType.Directory)
        {
            ParseDirectory(reader, startPosition);
        }

        private void ParseDirectory(WzReader reader, long startPosition)
        {
            // 1: Reset the BaseStream position to the start position of the data
            reader.BaseStream.Seek(startPosition, SeekOrigin.Begin);

            // 2: The amount of WzObjects in this directory
            var objectCount = reader.ReadWzInt();

            // 3: Loop through all of the WzObjects in the directory
            for (int i = 0; i < objectCount; i++)
            {
                // 3.1: The WzObjectType of the WzObject
                var type = reader.ReadByte();
                string name = "";
                
                switch (type)
                {

                    case (byte)WzObjectType.Image:

                        name = reader.ReadWZString();
                        break;

                    default:
                        

                        break;


                }

                int size = reader.ReadWzInt();
                int checksum = reader.ReadWzInt();
                uint offset = reader.ReadOffset((uint)ParentFile.Header.StartPosition);

                //Console.WriteLine(size);
                //Console.WriteLine(checksum);
                //Console.WriteLine(offset);

                switch (type)
                {
                    case (byte)WzObjectType.Image:
                        Add(new WzImage(name, this, ParentFile, ParentFile.GetSubStream(offset)));
                        break;
                }

            }


            
        }
    }
}
