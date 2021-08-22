using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit
{
    /// <summary>
    /// Declares a method added to the ResolverContext as an
    /// explicit type conversion
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ImplicitArgumentAttribute : Attribute
    {
        public ImplicitArgumentAttribute()
        {
            Name = null;
        }

        public ImplicitArgumentAttribute(string name)
        {
            Name = name;
        }

        public string Name
        {
            get;
            private set;
        }
    }
}
