using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit
{
    internal class CodeException : Exception
    {
        public CodeException(string message, SourcePosition position = null) 
            : base(message)
        {
            Position = position;
        }

        public CodeException(string message, Exception innerException, SourcePosition position = null) 
            : base(message, innerException)
        {
            Position = position;
        }

        public SourcePosition Position { get; set; }
    }
}
