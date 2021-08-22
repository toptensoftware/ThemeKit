using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Runtime
{
    /// <summary>
    /// Manages the set of arguments to a method call
    /// </summary>
    internal class MethodCallContender
    {
        /// <summary>
        /// Constructs a new method call
        /// </summary>
        /// <param name="methodInfo">The method info</param>
        /// <param name="instance">An object object instance to invoke the method on</param>
        public MethodCallContender(MethodInfo methodInfo, Object instance)
        {
            _mi = methodInfo;
            _instance = instance;
            _parameters = _mi.GetParameters().Select(x => new Parameter()
            {
                Info = x,
            }).ToArray();

            _explicitParameters = _parameters.Where(x => x.ImplicitArgumentName == null).ToArray();
        }

        /// <summary>
        /// Gets the method info for this call contender
        /// </summary>
        public MethodInfo MethodInfo => _mi;

        /// <summary>
        /// Gets the return type of the target method
        /// </summary>
        public Type ReturnType => _mi.ReturnType;

        /// <summary>
        /// Gets the current parameter state of all parameters
        /// </summary>
        public Parameter[] Parameters => _parameters;

        /// <summary>
        /// Gets the explicit parameters (ie: those that are implicit)
        /// </summary>
        public Parameter[] ExplicitParameters => _explicitParameters;

        /// <summary>
        /// Apply any implicit arguments
        /// </summary>
        /// <param name="argValues">The dictionary of implicit arg values</param>
        public void ApplyImplicitArguments(IDictionary<string, object> argValues)
        {
            foreach (var p in _parameters)
            {
                var ia = p.ImplicitArgumentName;
                if (ia != null)
                {
                    if (argValues.TryGetValue(ia, out var argValue))
                    {
                        p.Value = argValue;
                    }
                }
            }
        }

        public bool HasUnresolveImplicitParameters => _parameters.Any(x => !x.Resolved && x.ImplicitArgumentName != null);

        public bool CanAcceptArgumentCount(int argCount)
        {
            // What's the max we can accept?
            if (argCount > _explicitParameters.Length)
                return false;

            // What's the minimum we need?
            int minArgs = _explicitParameters.Where(x => !x.Info.HasDefaultValue).Count();
            if (argCount < minArgs)
                return false;

            // It's good
            return true;
        }

        /// <summary>
        /// Check if any parameters have an exception
        /// </summary>
        public bool HasErrors => _parameters.Any(x => x.Exception != null);

        /// <summary>
        /// Check if the method can be invoked because all parameters
        /// have been resolved
        /// </summary>
        public bool CanInvoke => _parameters.All(x => x.Resolved || x.Info.HasDefaultValue);

        /// <summary>
        /// Invoke the method
        /// </summary>
        /// <returns></returns>
        public object Invoke()
        {
            // Assign default values to any parameters that haven't yet been resolved.
            foreach (var p in _parameters.Where(x=>!x.Resolved && x.Info.HasDefaultValue))
            {
                p.Value = p.Info.DefaultValue;
            }

            // Invoke it
            return _mi.Invoke(_instance, _parameters.Select(x => x.Value).ToArray());
        }

        /// <summary>
        /// Stores information about a parameter
        /// </summary>
        public class Parameter
        {
            /// <summary>
            /// The ParameterInfo for this parameter
            /// </summary>
            public ParameterInfo Info;

            /// <summary>
            /// The current value of this parameter
            /// </summary>
            public Object Value
            {
                get => _value;
                set
                {
                    _value = value;
                    _resolved = true;
                }
            }

            /// <summary>
            /// Gets or sets an exception associated with evaluating
            /// this parameter
            /// </summary>
            public Exception Exception
            {
                get;
                set;
            }

            Object _value;
            bool _resolved;

            /// <summary>
            /// True if the parameter has been resolved
            /// </summary>
            public bool Resolved
            {
                get => _resolved;
            }

            /// <summary>
            /// Gets the name of the implicit argument
            /// </summary>
            public string ImplicitArgumentName
            {
                get
                {
                    var ia = Info.GetCustomAttribute<ImplicitArgumentAttribute>();
                    if (ia != null)
                    {
                        return ia.Name ?? Info.Name;
                    }
                    return null;
                }
            }
        }

        MethodInfo _mi;
        Object _instance;
        Parameter[] _parameters;
        Parameter[] _explicitParameters;
    }
}
