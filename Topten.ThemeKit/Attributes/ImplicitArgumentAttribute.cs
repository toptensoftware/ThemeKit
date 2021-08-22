using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit
{
    /// <summary>
    /// Declares that a parameter of a method is an implicit argument
    /// and should be resolved using the Theme.ImplicitArgumentValues collection
    /// rather than from the call site arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ImplicitArgumentAttribute : Attribute
    {
        /// <summary>
        /// Constructs a new implicit argument attribute, using the 
        /// parameter name as the name of the lookup.
        /// </summary>
        public ImplicitArgumentAttribute()
        {
            Name = null;
        }

        /// <summary>
        /// Constructs a new implicit argument attribute, with an explicit
        /// name for the argument lookup
        /// </summary>
        public ImplicitArgumentAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the argument name associated with this implicit parameter
        /// </summary>
        public string Name
        {
            get;
            private set;
        }
    }
}
