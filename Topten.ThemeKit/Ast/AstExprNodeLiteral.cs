using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal class AstExprNodeLiteral : AstExprNode
    {
        public AstExprNodeLiteral(SourcePosition pos, object value) : base(pos)
        {
            Value = value;
        }

        public object Value { get; }

        public override T Visit<T>(IAstExprNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}
