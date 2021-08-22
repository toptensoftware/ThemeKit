using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Topten.ThemeKit.Runtime
{
    /// <summary>
    /// Selects a method to call
    /// </summary>
    internal class MethodCallSelector
    {
        /// <summary>
        /// Constructs a new method selector
        /// </summary>
        public MethodCallSelector(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Adds a method overload contender
        /// </summary>
        /// <param name="mi">The MethodInfo for the method</param>
        /// <param name="instance">The object instance if non-static member</param>
        public void AddContender(MethodInfo mi, Object instance)
        {
            _contenders.Add(new MethodCallContender(mi, instance));
        }

        /// <summary>
        /// Sets the implicit args on all contenders 
        /// </summary>
        /// <param name="argValues">A dictionary of implicit argument values</param>
        public void ApplyImplicitArguments(IDictionary<string, object> argValues)
        {
            foreach (var c in _contenders)
            {
                c.ApplyImplicitArguments(argValues);
            }
        }

        /// <summary>
        /// Removes contender functions that don't have all implicit parameters resolved
        /// </summary>
        public void RemoveContendersWithUnresolvedImplicitParameters()
        {
            _totalOverloadCount = _contenders.Count;
            _contenders.RemoveAll(x => x.HasUnresolveImplicitParameters);
        }

        /// <summary>
        /// Removes contender functions that don't have the required number of parameters
        /// </summary>
        /// <param name="availableArguments"></param>
        public void RemoveContendersByArgumentCount(int availableArguments)
        {
            _contenders.RemoveAll(x => !x.CanAcceptArgumentCount(availableArguments));
            _callerArgumentCount = availableArguments;
        }

        /// <summary>
        /// Gets the various parameter type overloads at a parameter positio
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter</param>
        /// <returns>An enumeration of the different parameter types available at this index</returns>
        public IEnumerable<Type> GetParameterTypes(int parameterIndex)
        {
            return _contenders
                .Where(x => !x.HasErrors)
                .Select(x => x.ExplicitParameters[parameterIndex].Info.ParameterType)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Sets the argument value for all contenders with a parameter
        /// of specified type at the specified index
        /// </summary>
        /// <param name="parameterIndex">Index of the parameter to set</param>
        /// <param name="type">The type of parameters to set</param>
        /// <param name="value">The argument value</param>
        public void SetArgument(int parameterIndex, Type type, object value)
        {
            foreach (var c in _contenders)
            {
                if (c.ExplicitParameters[parameterIndex].Info.ParameterType == type)
                {
                    c.ExplicitParameters[parameterIndex].Value = value;
                }
            }
        }

        /// <summary>
        /// Remove all contenders that have a particular parameter type
        /// </summary>
        /// <param name="parameterIndex">The index of the parameter to check</param>
        /// <param name="type">The parameter type</param>
        public void SetArgumentError(int parameterIndex, Type type, Exception exception)
        {
            foreach (var c in _contenders)
            {
                if (c.ExplicitParameters[parameterIndex].Info.ParameterType == type)
                {
                    c.ExplicitParameters[parameterIndex].Exception = exception;
                }
            }
        }

        /// <summary>
        /// Checks if the call can be invoked
        /// </summary>
        public bool CanInvoke => _contenders.Where(x=>x.CanInvoke).SingleOrDefault() != null;

        /// <summary>
        /// Invokes the selected target method
        /// </summary>
        /// <returns>The return value from the method</returns>
        public object Invoke(SourcePosition callsite)
        {
            // No contenders to start with?
            if (_contenders.Count == 0)
            {
                if (_totalOverloadCount == 1)
                {
                    throw new CodeException($"The function '{_name}' doesn't take {_callerArgumentCount} arguments", callsite);
                }
                else
                {
                    throw new CodeException($"No overload of '{_name}' takes {_callerArgumentCount} arguments", callsite);
                }
            }
            
            // Find the winners
            var winners = _contenders.Where(x => x.CanInvoke);

            // Was there just one?
            var winner = winners.SingleOrDefault();
            if (winner != null)
            {
                try
                {
                    return winner.Invoke();
                }
                catch (Exception x)
                {
                    throw new CodeException($"Error invoking {winner.MethodInfo.ToString()} - {x.Message}", x, callsite);
                }
            }

            if (winners.Any())
            {
                throw new CodeException($"Couldn't resolve call to '{_name}' based on arguments.", callsite);
            }

            if (_contenders.Count > 1)
            {
                throw new CodeException($"No suitable function overload for {_name} found.", callsite);
            }
            else
            {
                for (int i = 0; i < _contenders[0].ExplicitParameters.Length; i++)
                {
                    var exception = _contenders[0].ExplicitParameters[i].Exception;
                    if (exception != null)
                    {
                        throw new CodeException($"Error in parameter {i} calling '{_contenders[0].MethodInfo.ToString()}'", exception, callsite);
                    }
                }
            }

            throw new CodeException($"Unknown error resolving function overload", callsite);
        }


        /// <summary>
        /// A list of method contenders
        /// </summary>
        List<MethodCallContender> _contenders = new List<MethodCallContender>();
        int _totalOverloadCount;
        int _callerArgumentCount;
        string _name;
    }
}
