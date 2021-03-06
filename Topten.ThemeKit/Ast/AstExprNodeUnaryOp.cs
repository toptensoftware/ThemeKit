using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topten.ThemeKit.Runtime;

namespace Topten.ThemeKit.Ast
{
    internal class AstExprNodeUnaryOp : AstExprNode
    {
        public AstExprNodeUnaryOp(SourcePosition pos) : base(pos)
        {
        }

        public OperatorType Operator;

        public AstExprNode RHS;

        public override T Visit<T>(IAstExprNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}
