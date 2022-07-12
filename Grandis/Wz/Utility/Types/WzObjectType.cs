using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grandis.Wz.Utility.Types
{
    public enum WzObjectType : byte
    {
        Directory = 0,
        Image = 4,
        Null = 2,
        UShort = 3,
        Int = 4,
        Long = 14,
        Single = 5,
        Double = 6,
        String = 7,
        Point = 8,
        UOL = 9,
        Audio = 10,
        Canvas = 11,
        SubProperty = 12,
        Convex = 13
    }
}
