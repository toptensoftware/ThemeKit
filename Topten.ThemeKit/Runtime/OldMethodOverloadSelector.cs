using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#if NOPE
namespace Topten.ThemeKit
{
    static class OldMethodOverloadSelector
    {
        /// <summary>
        /// Check if a type can be implicitly converted to another type
        /// </summary>
        /// <remarks>
        /// Based on this: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/implicit-numeric-conversions-table
        /// </remarks>
        /// <param name="typeDest"></param>
        /// <param name="typeSource"></param>
        /// <returns></returns>
        public static bool IsImplicitlyConvertible(Type typeDest, Type typeSource)
        {
            var tcDest = Type.GetTypeCode(typeDest);
            var tcSrc = Type.GetTypeCode(typeSource);
            switch (tcSrc)
            {
                case TypeCode.SByte:
                    switch (tcDest)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Byte:
                    switch (tcDest)
                    {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Char:
                    switch (tcDest)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Int16:
                    switch (tcDest)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.UInt16:

                    switch (tcDest)
                    {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Int32:
                    switch (tcDest)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.UInt32:
                    switch (tcDest)
                    {
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch (tcDest)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;
                case TypeCode.Single:
                    if (tcDest == TypeCode.Double)
                        return true;
                    break;
            }

            return false;
        }

        /// <summary>
        /// Calculate a compatibility score for type conversion
        /// </summary>
        /// <remarks>
        /// Based on this: https://stackoverflow.com/a/14336566/77002
        /// </remarks>
        /// <param name="dst">The destination type</param>
        /// <param name="types">Type base, or interfaces implemented by the source type</param>
        /// <returns>A score, where higher is less compatible</returns>
        static int CompatibilityScore(Type dst, IEnumerable<Type> types)
        {
            int score = 0;
            foreach (var t in types)
            {
                if (dst.IsAssignableFrom(t))
                {
                    score++;
                }
            }
            return score;
        }

        /// <summary>
        /// Calculate a compatibility score for an argument to 
        /// a parameter type
        /// </summary>
        /// <param name="typeParameter">The parameter type</param>
        /// <param name="typeArgument">The argument type (or null if use default)</param>
        /// <returns></returns>
        public static int CalculateParameterScore(Type typeParameter, Type typeArgument)
        {
            // Exact match?
            if (typeParameter == typeArgument)
                return 0;

            // Null parameter?
            if (typeArgument == null)
                return typeParameter.IsValueType ? -1 : 0;

            // Implicitly convertible?
            if (IsImplicitlyConvertible(typeParameter, typeArgument))
                return 2;

            // Calculate compatibility score
            int compatibilityScore = CompatibilityScore(typeParameter, typeArgument.GetInterfaces()) +
                     CompatibilityScore(typeParameter, new Type[] { typeArgument.BaseType }) +
                     CompatibilityScore(typeParameter, new Type[] { typeArgument });

            // If the score is zero then it's not assignable at all
            if (compatibilityScore == 0)
                return -1;

            return compatibilityScore;
        }

        public class NamedArgInfo
        {
            public Type type;          // The argument type
            public bool optional;      // Whether argument must be used
        }


        /// <summary>
        /// Calculate the method score for matching a set of positional and named
        /// argument types to a method
        /// </summary>
        /// <param name="mi">The method to calculate the score for</param>
        /// <param name="positionalArguments">The positional argument types</param>
        /// <param name="namedArguments">The named argument types</param>
        /// <returns>A match score, lower is better, -1 if doesn't match at all</returns>
        static int CalculateMethodScore(MethodInfo mi, Type[] positionalArguments, Dictionary<string, NamedArgInfo> namedArguments)
        {
            // Get method's parameters
            var parameters = mi.GetParameters();

            // Check we dont have too many parameters
            int totalArgs = positionalArguments.Length + (namedArguments == null ? 0 : namedArguments.Count);

            // Calculate sum of all positional arguments
            int score = 0;
            for (int i = 0; i < positionalArguments.Length; i++)
            {
                if (i >= parameters.Length)
                    return -1;

                // Get the parameter score and quit if not compatible.
                var paramScore = CalculateParameterScore(parameters[i].ParameterType, positionalArguments[i]);
                if (paramScore < 0)
                    return -1;

                // Update score
                score += paramScore;
            }

            // Now match the named arguments
            int usedNamedArgCount = 0;
            for (int i = positionalArguments.Length; i < parameters.Length; i++)
            {
                // Do we have a named argument for it?
                if (namedArguments != null && namedArguments.TryGetValue(parameters[i].Name, out var namedArgInfo))
                {
                    // Check it's compatible
                    var paramScore = CalculateParameterScore(parameters[i].ParameterType, namedArgInfo.type);
                    if (paramScore < 0)
                        return -1;

                    // Update method score
                    score += paramScore;

                    // Count used named args
                    if (!namedArgInfo.optional)
                        usedNamedArgCount++;
                }
                else
                {
                    // Is there a default value for this unspecified arg?
                    if (!parameters[i].HasDefaultValue)
                        return -1;

                    // Using a default? Add 3 to score (don't need to call CalculateParameterScore)
                    score += 3;
                }
            }

            // Check all named arguments were used
            if (namedArguments != null && namedArguments.Where(x=>!x.Value.optional).Count() != usedNamedArgCount)
                return -1;

            return score;
        }

        /// <summary>
        /// Given a set of method overloads, pick the best one based
        /// on parameter types
        /// </summary>
        /// <param name="methods">The methods to choose from</param>
        /// <param name="positionalArguments">Position argument types</param>
        /// <param name="namedArguments">Name argument types</param>
        /// <returns>The best method overload, or null if none</returns>
        public static MethodInfo ChooseMethodOverload(IEnumerable<MethodInfo> methods, Type[] positionalArguments, Dictionary<string, NamedArgInfo> namedArguments)
        {
            // Track the best match
            MethodInfo miBest = null;
            int scoreBest = int.MaxValue;
            bool ambiguousBest = false;

            // Check each method
            foreach (var mi in methods)
            {
                // Calculate score
                int score = CalculateMethodScore(mi, positionalArguments, namedArguments);

                // Not compatible?
                if (score < 0)
                    continue;

                // Duplicate best score?  Flag it, we'll need to find a better one
                if (score == scoreBest)
                {
                    ambiguousBest = true;
                    continue;
                }

                // Better score?
                if (score < scoreBest)
                {
                    ambiguousBest = false;
                    scoreBest = score;
                    miBest = mi;
                }
            }

            // If ambiguous then no match
            if (ambiguousBest)
                throw new InvalidOperationException($"Ambiguous method call: {miBest.Name}");

            // Found?
            if (miBest == null)
            {
                int count = methods.Count();
                if (count == 0)
                    throw new InvalidOperationException("Method not found");

                if (count == 1)
                    throw new InvalidOperationException($"Method parameter mismatch");

                throw new InvalidOperationException($"No suitable method overload found");
            }

            // Best or nothing matched
            return miBest;
        }

        /// <summary>
        /// Invoke a previously matched method
        /// </summary>
        /// <param name="mi">The method to invoke</param>
        /// <param name="instance">The object instance to invoke (or null for static)</param>
        /// <param name="positionalArguments">The position arguments</param>
        /// <param name="namedArguments">The named arguments</param>
        /// <returns>The return value from the invoked method</returns>
        public static object InvokeMethod(MethodInfo mi, object instance, object[] positionalArguments, Dictionary<string, object> namedArguments)
        {
            // Build converted arguments into this list
            var allArgs = new List<object>();

            // Get the methods paramters
            var parameters = mi.GetParameters();

            // Add positional arguments
            if (positionalArguments != null)
            {
                for (int i = 0; i < positionalArguments.Length; i++)
                {
                    if (parameters[i].ParameterType.IsAssignableFrom(positionalArguments[i].GetType()))
                        allArgs.Add(positionalArguments[i]);
                    else
                        allArgs.Add(Convert.ChangeType(positionalArguments[i], parameters[i].ParameterType));
                }
            }

            // Add default and named arguments
            for (int i = positionalArguments.Length; i < parameters.Length; i++)
            {
                // Do we have a named argument for it?
                if (namedArguments != null && namedArguments.TryGetValue(parameters[i].Name, out var namedArg))
                {
                    if (parameters[i].ParameterType.IsAssignableFrom(namedArg.GetType()))
                        allArgs.Add(namedArg);
                    else
                        allArgs.Add(Convert.ChangeType(namedArg, parameters[i].ParameterType));
                }
                else
                {
                    allArgs.Add(parameters[i].DefaultValue);
                }
            }

            // Invoke it!
            return mi.Invoke(instance, allArgs.ToArray());
        }
    }
}
#endif