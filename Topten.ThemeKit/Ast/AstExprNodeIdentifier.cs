using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal class AstExprNodeIdentifier : AstExprNode
    {
        public AstExprNodeIdentifier(SourcePosition pos, string name) : base(pos)
        {
            Name = name ;
        }

        public string Name { get; }

        //public AstExprNode LHS { get; set;  }

        public override T Visit<T>(IAstExprNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}
