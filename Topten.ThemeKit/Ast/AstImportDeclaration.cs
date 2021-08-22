using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal class AstImportDeclaration : AstElement
    {
        public AstImportDeclaration(SourcePosition pos, string name) : base(pos)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
