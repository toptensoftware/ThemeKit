using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal class AstExprNodeTernaryOp : AstExprNode
    {
        public AstExprNodeTernaryOp(SourcePosition pos) : base(pos)
        {
        }

        public AstExprNode Condition;
        public AstExprNode TrueNode;
        public AstExprNode FalseNode;

        public override T Visit<T>(IAstExprNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}
