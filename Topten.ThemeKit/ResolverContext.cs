using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Topten.ThemeKit.Runtime;

namespace Topten.ThemeKit
{
    /// <summary>
    /// Provides a context to resolve function calls and implicit conversions
    /// </summary>
    public class ResolverContext
    {
        /// <summary>
        /// Constructs a new resolver context
        /// </summary>
        public ResolverContext()
        {
        }

        /// <summary>
        /// Registers a method that can be called from user tkl file
        /// </summary>
        /// <param name="mi">The MethodInfo for the method</param>
        /// <param name="instance">An object instance to be used for non-static methods</param>
        public void AddMethod(MethodInfo mi, object instance)
        {
            var entry = new Entry()
            {
                mi = mi,
                instance = instance,
            };

            // Implicit conversion function
            if (mi.GetCustomAttribute<ImplicitConversionAttribute>() != null)
            {
                _implicitConversions.Add(entry);
                return;
            }

            // Get collection of methods by name
            if (!_methodsByName.TryGetValue(mi.Name, out var list))
            {
                list = new List<Entry>();
                _methodsByName.Add(mi.Name, list);
            }

            // Add entry
            list.Add(entry);
        }

        /// <summary>
        /// Registers all the public static methods from a type
        /// </summary>
        /// <param name="type">The type whose static methods are to be registered</param>
        public void AddStaticMethods(Type type)
        {
            foreach (var mi in type.GetMethods().Where(x => x.IsPublic && !x.IsSpecialName && x.IsStatic))
            {
                AddMethod(mi, null);
            }
        }

        /// <summary>
        /// Registers all instance methods from an object
        /// </summary>
        /// <param name="obj">The object instance to be registered</param>
        public void AddInstanceMethods(object obj)
        {
            foreach (var mi in obj.GetType().GetMethods().Where(x => x.IsPublic && !x.IsSpecialName && !x.IsStatic))
            {
                AddMethod(mi, obj);
            }
        }

        internal MethodCallSelector GetFunctionsByName(string name)
        {
            // Get methods with this name
            if (!_methodsByName.TryGetValue(name, out var entries))
                return null;

            // Setup a selector
            var sel = new MethodCallSelector(name);
            foreach (var e in entries)
            {
                sel.AddContender(e.mi, e.instance);
            }

            return sel;
        }

        internal MethodCallSelector GetImplicitConversions(Type targetType)
        {
            if (targetType == null)
                return null;

            // Find contenders
            MethodCallSelector sel = null;
            foreach (var e in _implicitConversions.Where(x => x.mi.ReturnType == targetType))
            {
                if (sel == null)
                    sel = new($"<implicit conversion to '{targetType}'>");
                sel.AddContender(e.mi, e.instance);
            }

            // None?
            if (sel == null)
                return null;

            return sel;
        }

        class Entry
        {
            public MethodInfo mi;
            public object instance;
        }

        Dictionary<string, List<Entry>> _methodsByName = new Dictionary<string, List<Entry>>();
        List<Entry> _implicitConversions = new List<Entry>();

    }
}
