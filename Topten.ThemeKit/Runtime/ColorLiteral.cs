using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Runtime
{
    /// <summary>
    /// Represents a Color #RRGGBB literal parsed from a tkl file
    /// </summary>
    public struct ColorLiteral
    {
        /// <summary>
        /// Constructs a new ColorLiteral from an ARGB value
        /// </summary>
        /// <param name="argb"></param>
        public ColorLiteral(uint argb)
        {
            ARGB = argb;
        }

        /// <summary>
        /// The ARGB value of this color literal
        /// </summary>
        public uint ARGB;
    }
}
