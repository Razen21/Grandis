using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grandis.Wz.Utility
{
    [Serializable]
    public class WzException : Exception
    {
        internal WzException(string context = "", Exception innerException = null!) : base(context, innerException) { }
    }
}
