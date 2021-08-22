using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Runtime
{
    /// <summary>
    /// Represents an identifier literal
    /// </summary>
    public struct IdentifierLiteral
    {
        /// <summary>
        /// Constructs a new identifier literal
        /// </summary>
        /// <param name="identifier">The identifier name</param>
        public IdentifierLiteral(string identifier)
        {
            Identifier = identifier;
        }

        /// <summary>
        /// Gets the identifier name
        /// </summary>
        public string Identifier
        {
            get;
            private set;
        }
    }
}
