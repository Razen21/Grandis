using Grandis.Wz.Utility;
using Grandis.Wz.Utility.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grandis.Wz.Objects.Property
{
    public abstract class WzProperty<T> : WzObject
    {
        protected T value;

        internal WzProperty(string name, WzObject parent, T value, WzImage image, bool canContainChildren, WzObjectType type) 
            : base(name, parent, image.ParentFile, canContainChildren, type)
        {
            this.value = value;
            Image = image;

        }
        public virtual T Value => value;
        public WzImage Image { get; }
    }

    public abstract class WzDelayedProperty<T> : WzProperty<T>
    {
        private readonly long offset;
        private readonly WzReader reader;

        protected bool isParsed;

        public WzDelayedProperty(string name, WzObject parent, WzImage parentImage, WzReader reader, bool canContainChildren, WzObjectType type)
            : base(name, parent, default(T)!, parentImage, canContainChildren, type)
        {
            offset = reader.BaseStream.Position;
            isParsed = Parse(reader, true, out value);
            this.reader = reader;

        }

        public override T Value
        {
            get
            {
                if (!isParsed)
                {
                    CheckParsed();
                }
                return value;
            }
        }

        internal void CheckParsed()
        {
            if (isParsed)
            {
                return;
            }
            WzReader _reader = reader.Clone();
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            Parse(_reader, false, out value);
        }

        internal abstract bool Parse(WzReader reader, bool initial, out T result);
    }

    public struct WzNothing { }

    public sealed class WzNullProperty : WzProperty<WzNothing> 
    {
        public WzNullProperty(string name, WzObject parent, WzImage parentImage)
            : base(name, parent, default(WzNothing), parentImage, false, WzObjectType.Null) {}
    }

    public sealed class WzUShortProperty : WzProperty<ushort>
    {
        internal WzUShortProperty(string name, WzObject parent, WzReader reader, WzImage container)
            : base(name, parent, reader.ReadUInt16(), container, false, WzObjectType.UShort) { }
    }

    public sealed class WzIntProperty : WzProperty<int>
    {
        internal WzIntProperty(string name, WzObject parent, WzReader reader, WzImage container)
            : base(name, parent, reader.ReadWzInt(), container, false, WzObjectType.Int) { }
    }


    public sealed class WzLongProperty : WzProperty<long>
    {
        internal WzLongProperty(string name, WzObject parent, WzReader reader, WzImage container)
            : base(name, parent, reader.ReadWzLong(), container, false, WzObjectType.Long) { }
    }


    public sealed class WzSingleProperty : WzProperty<float>
    {
        internal WzSingleProperty(string name, WzObject parent, WzReader reader, WzImage container)
            : base(name, parent, ReadSingle(reader), container, false, WzObjectType.Single) { }

        private static float ReadSingle(WzReader reader)
        {
            byte t = reader.ReadByte();
            return t == 0x80
                ? reader.ReadSingle()
                : (t == 0 ? 0f : 0f);
        }
    }


    public sealed class WzDoubleProperty : WzProperty<double>
    {
        internal WzDoubleProperty(string name, WzObject parent, WzReader reader, WzImage container)
            : base(name, parent, reader.ReadDouble(), container, false, WzObjectType.Double) { }
    }

    public sealed class WzStringProperty : WzDelayedProperty<string>
    {
        internal WzStringProperty(string name, WzObject parent, WzReader reader, WzImage container)
            : base(name, parent, container, reader, false, WzObjectType.String) { }

        internal override bool Parse(WzReader r, bool initial, out string result)
        {
            result = string.Intern(r.ReadWzStringBlock(true));
            Console.WriteLine(result);
            return true;


            
           
        }
    }





    public static class PropertyParser
    {
        public static List<WzObject> ParsePropertyList(WzReader reader, WzObject parent, WzImage parentImage, bool encrypted)
        {
            
            int propertyCount = reader.ReadWzInt();
            List<WzObject> properties = new List<WzObject>();

            for (int i = 0; i < propertyCount; i++)
            {
                string name = reader.ReadWzStringBlock(encrypted);
                byte propertyType = reader.ReadByte();

                Console.WriteLine(propertyType);

                switch(propertyType)
                {
                    case 0:
                        properties.Add(new WzNullProperty(name, parent, parentImage));
                        break;
                    case 0x0B:
                    case 2:
                        properties.Add(new WzUShortProperty(name, parent, reader, parentImage));
                        break;
                    case 0x13:
                    case 3:
                        properties.Add(new WzIntProperty(name, parent, reader, parentImage));
                        break;
                    case 0x14:
                        properties.Add(new WzLongProperty(name, parent, reader, parentImage));
                        break;
                    case 4:
                        properties.Add(new WzSingleProperty(name, parent, reader, parentImage));
                        break;
                    case 5: 
                        properties.Add(new WzDoubleProperty(name, parent, reader, parentImage));
                        break;
                    case 8: 
                        properties.Add(new WzStringProperty(name, parent, reader, parentImage));
                        break;

                    
                }
            }

            return properties;
        }
    }
}
