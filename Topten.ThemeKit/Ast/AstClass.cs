using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Ast
{
    internal class AstClass : AstExprNodeDictionary
    {
        public AstClass(SourcePosition pos, string className) : base(pos)
        {
            ClassName = className;
        }

        public string ClassName { get; }

        public class BaseClassEntry
        {
            public string ClassName { get; set; }
        }

        List<BaseClassEntry> _baseClasses = new List<BaseClassEntry>();

        public void AddBase(string className)
        {
            _baseClasses.Add(new BaseClassEntry()
            {
                ClassName = className,
            });
        }

        public IEnumerable<BaseClassEntry> BaseClasses => _baseClasses;
    }
}
