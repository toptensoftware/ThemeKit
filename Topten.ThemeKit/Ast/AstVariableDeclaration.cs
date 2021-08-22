using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal class AstVariableDeclaration : AstElement
    {
        public AstVariableDeclaration(SourcePosition pos, string name, AstExprNode value) : base(pos)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public AstExprNode Value { get; }
    }
}
