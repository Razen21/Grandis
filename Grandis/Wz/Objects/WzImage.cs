using Grandis.Wz.Objects.Property;
using Grandis.Wz.Utility;
using Grandis.Wz.Utility.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grandis.Wz.Objects
{
    public class WzImage : WzObject
    {
        private readonly WzReader reader;
        private bool _parsed;


        public WzImage(string name, WzObject parent, WzFile parentFile, WzReader reader) 
            : base(name, parent, parentFile, true, WzObjectType.Image)
        {
            this.reader = reader;

           
            ParseImage();
        }

        public override WzObject this[string childName]
        {
            get
            {
                if (!_parsed)
                {
                    ParseImage();
                }
                return base[childName];
            }
        }

        public override int ChildCount
        {
            get
            {
                if (!_parsed)
                {
                    ParseImage();
                }
                return base.ChildCount;
            }
        }

        public override IEnumerator<WzObject> GetEnumerator()
        {
            if (!_parsed)
            {
                ParseImage();
            }
            return base.GetEnumerator();
        }

        public override bool HasChild(string name)
        {
            if (!_parsed)
            {
                ParseImage();
            }
            return base.HasChild(name);
        }

        private void ParseImage()
        {

            reader.ReadWzStringBlock(true);
            reader.ReadUInt16();

            PropertyParser.ParsePropertyList(reader, this, this, true);

            
        }
    }
}
