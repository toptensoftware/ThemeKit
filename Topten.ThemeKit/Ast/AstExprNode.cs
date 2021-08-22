using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal interface IAstExprNodeVisitor<T>
    {
        T Visit(AstExprNodeLiteral el);
        T Visit(AstExprNodeUnaryOp el);
        T Visit(AstExprNodeBinaryOp el);
        T Visit(AstExprNodeTernaryOp el);
        T Visit(AstExprNodeList el);
        T Visit(AstExprNodeIdentifier el);
        T Visit(AstExprNodeFunctionCall el);
        T Visit(AstExprNodeDictionary el);
    }

    internal abstract class AstExprNode : AstElement
    {
        public AstExprNode(SourcePosition pos) : base(pos)
        {

        }
        public abstract T Visit<T>(IAstExprNodeVisitor<T> visitor);
    }
}
