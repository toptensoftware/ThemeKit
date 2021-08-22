using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal class AstExprNodeDictionary : AstExprNode
    {
        public AstExprNodeDictionary(SourcePosition pos) : base(pos)
        {
        }

        public void Add(SourcePosition position, AstExprNode key, AstExprNode value)
        {
            _elements.Add(new Element()
            {
                Position = position,
                Key = key,
                Value = value,
            });
        }

        public class Element
        {
            public SourcePosition Position;
            public AstExprNode Key;
            public AstExprNode Value;
        }

        List<Element> _elements = new List<Element>();

        public IEnumerable<Element> Elements => _elements;

        public override T Visit<T>(IAstExprNodeVisitor<T> visitor) => visitor.Visit(this);
    }
}
