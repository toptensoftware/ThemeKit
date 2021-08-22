using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal class AstDocument : AstElement
    {
        public AstDocument(SourcePosition pos) : base(pos)
        {

        }

        List<AstElement> _elements = new List<AstElement>();

        public void Add(AstElement element)
        {
            _elements.Add(element);
        }

        public IEnumerable<AstElement> Elements => _elements;
    }
}
