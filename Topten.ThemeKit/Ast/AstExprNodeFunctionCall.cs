using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal class AstExprNodeFunctionCall : AstExprNode
    {
        public AstExprNodeFunctionCall(SourcePosition pos, string name) : base(pos)
        {
            Name = name ;
        }

        public string Name { get; }

        public void AddArgument(AstExprNode argument)
        {
            _arguments.Add(argument);
        }

        List<AstExprNode> _arguments = new List<AstExprNode>();

        public List<AstExprNode> Arguments => _arguments;

        public override T Visit<T>(IAstExprNodeVisitor<T> visitor) => visitor.Visit(this);

    }

}
