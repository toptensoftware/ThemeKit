using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal abstract class AstElement
    {
        public AstElement(SourcePosition position)
        {
            Position = position;
        }

        public SourcePosition Position { get; }
    }
}
