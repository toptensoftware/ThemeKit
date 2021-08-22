using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal class AstExprNodeList : AstExprNode
    {
        public AstExprNodeList(SourcePosition pos) : base(pos)
        {
        }

        public void Add(AstExprNode value)
        {
            _elements.Add(value);
        }

        public IEnumerable<AstExprNode> Elements => _elements;

        List<AstExprNode> _elements = new List<AstExprNode>();

        public override T Visit<T>(IAstExprNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}
