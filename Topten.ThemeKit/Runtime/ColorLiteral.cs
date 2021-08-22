using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Runtime
{
    public struct ColorLiteral
    {
        public ColorLiteral(uint argb)
        {
            ARGB = argb;
        }

        public uint ARGB;
    }
}
