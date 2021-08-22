using Topten.ThemeKit.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Topten.ThemeKit.Runtime;

namespace Topten.ThemeKit
{
    internal interface IResolverSite
    {
        ResolverContext GetResolverContext();
        IDictionary<string, object> GetImplicitArgValues();
    }

    internal class Resolver : IAstExprNodeVisitor<object>
    {
        public Resolver(IResolverSite site)
        {
            _site = site;
        }

        IResolverSite _site;

        public IResolverSite Site => _site;

        public ResolverContext Context => _site.GetResolverContext();

        public IDictionary<string, object> ImplicitArgValues => _site.GetImplicitArgValues();

        // Add a theme document to this resolver
        public void AddDocument(AstDocument document)
        {
            if (document == null)
                return;
            foreach (var e in document.Elements)
            {
                if (e is AstVariableDeclaration varDecl)
                {
                    // Add tov variable map
                    _variables[varDecl.Name] = varDecl.Value;
                }
                else if (e is AstClass clsDecl)
                {
                    // Add to list of classes
                    if (!_classes.TryGetValue(clsDecl.ClassName, out var clsList))
                    {
                        clsList = new List<AstClass>();
                        _classes.Add(clsDecl.ClassName, clsList);
                    }
                    clsList.Add(clsDecl);
                }
                else if (e is AstImportDeclaration impDecl)
                {
                    // Ignore
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        // Load a themed class from this resolver
        public T GetClass<T>(string className) where T : class, new()
        {
            // Create the object
            var obj = new T();

            ApplyClasses(obj, className);

            return obj;
        }

        public IResourceProvider ResourceProvider
        {
            get;
            set;
        }

        // Apply classes to an object
        void ApplyClasses(object obj, string className)
        {
            // Find the list of class blocks that apply
            if (!_classes.TryGetValue(className, out var clsList))
                return;

            // Find all entries that match the style (or have no specified style)
            foreach (var cls in clsList)
            {
                // Recursively apply the base classes
                foreach (var b in cls.BaseClasses)
                {
                    ApplyClasses(obj, b.ClassName);
                }

                ApplyDictionaryToObject(obj, cls);
            }
        }

        Type _targetType;

        Exception TryEvaluateExprNode(AstExprNode node, Type targetType, out object value)
        {
            try
            {
                value = EvaluateExprNode(node, targetType);
                return null;
            }
            catch (Exception x)
            {
                value = null;
                return x;
            }
        }

        // Evaluate a node, with an optional implicit target type
        object EvaluateExprNode(AstExprNode node, Type targetType)
        {
            Type oldTargetType = _targetType;
            _targetType = targetType;
            try
            {
                // Evaluate the expression
                var value = node.Visit<object>(this);

                // Convert to requested type
                if (targetType != null && 
                    targetType != typeof(Object) &&
                    value != null && 
                    !value.GetType().IsAssignableTo(targetType)
                    )
                {
                    // Look for implicit conversions
                    var conversions = Context.GetImplicitConversions(_targetType);
                    if (conversions != null)
                    {
                        conversions.ApplyImplicitArguments(ImplicitArgValues);
                        conversions.RemoveContendersWithUnresolvedImplicitParameters();
                        conversions.RemoveContendersByArgumentCount(1);
                        foreach (var t in conversions.GetParameterTypes(0))
                        {
                            if (t.IsAssignableFrom(value.GetType()))
                            {
                                conversions.SetArgument(0, t, value);
                            }
                            else
                            {
                                conversions.SetArgumentError(0, t, new CodeException($"Unable to convert '{value.GetType()}' to '{t}'"));
                            }
                        }

                        if (conversions.CanInvoke)
                            return conversions.Invoke(node.Position);
                    }

                    // Try direct change type
                    try
                    {
                        return Operators.ImplicitChangeType(value, targetType);
                    }
                    catch (Exception x)
                    {
                        throw new CodeException($"Type mismatch: {x.Message}", node.Position);
                    }
                }

                // Done
                return value;
            }
            finally
            {
                _targetType = oldTargetType;
            }
        }

        void ApplyDictionaryToObject(object obj, AstExprNodeDictionary dict)
        {
            // Get target object type
            var t = obj.GetType();

            // Apply all elements
            foreach (var el in dict.Elements)
            {
                if (!(el.Key is AstExprNodeIdentifier id))
                    throw new CodeException("Expected identifier", el.Position);
                
                Type memberType;
                Func<object> getValue;
                Action<object> setValue;
                {
                    // Try to get the property or field
                    var pi = t.GetProperty(id.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    if (pi != null)
                    {
                        memberType = pi.PropertyType;
                        getValue = () => pi.GetValue(obj, null);
                        setValue = (val) => pi.SetValue(obj, val, null);
                    }
                    else
                    {
                        var fi = t.GetField(id.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                        if (fi != null)
                        {
                            memberType = fi.FieldType;
                            getValue = () => fi.GetValue(obj);
                            setValue = (val) => fi.SetValue(obj, val);
                        }
                        else
                        {
//                            Logging.Log.WriteLine($"{el.Position.Describe()}: Type {t.FullName} doesn't have a property '{el.Name}'");
                            continue;
                        }
                    }
                }

                // Evalute expression node
                var value = EvaluateExprNode(el.Value, memberType);

                // Assign to property
                try
                {
                    setValue(value);
                }
                catch (Exception x)
                {
                    throw new CodeException($"Error assigning to property or member '{id.Name}'", x, el.Position);
                }
            }
        }

        // List of variables
        Dictionary<string, AstExprNode> _variables = new Dictionary<string, AstExprNode>();

        // List of classes
        Dictionary<string, List<AstClass>> _classes = new Dictionary<string, List<AstClass>>();

        // Evaluate a literal node
        object IAstExprNodeVisitor<object>.Visit(AstExprNodeLiteral el)
        {
            // Too easy
            return el.Value;
        }

        object IAstExprNodeVisitor<object>.Visit(AstExprNodeUnaryOp el)
        {
            // If target type is enumerated type then evaluate
            // the operands as the target type too so enum identifiers
            // can be resolved
            if (_targetType != null && _targetType.IsEnum)
            {
                return Operators.ExplicitChangeType(
                    Operators.Evaluate(
                        el.Operator,
                        EvaluateExprNode(el.RHS, _targetType)
                        ), _targetType);
            }

            return Operators.Evaluate(
                el.Operator, 
                EvaluateExprNode(el.RHS, null)
                );
        }

        object IAstExprNodeVisitor<object>.Visit(AstExprNodeBinaryOp el)
        {
            // Short-circuit evaluation
            switch (el.Operator)
            {
                case OperatorType.LogicalAnd:
                    return (bool)EvaluateExprNode(el.LHS, typeof(bool)) && (bool)EvaluateExprNode(el.RHS, typeof(bool));

                case OperatorType.LogicalOr:
                    return (bool)EvaluateExprNode(el.LHS, typeof(bool)) || (bool)EvaluateExprNode(el.RHS, typeof(bool));
            }

            // Special case for operating on enums
            if (_targetType != null && _targetType.IsEnum)
            {
                return Operators.ExplicitChangeType(
                    Operators.Evaluate(
                        el.Operator,
                        EvaluateExprNode(el.LHS, _targetType),
                        EvaluateExprNode(el.RHS, _targetType)
                        ), _targetType);
            }

            // Invoke operator
            return Operators.Evaluate(
                el.Operator,
                EvaluateExprNode(el.LHS, null),
                EvaluateExprNode(el.RHS, null)
                );
        }

        object IAstExprNodeVisitor<object>.Visit(AstExprNodeTernaryOp el)
        {
            if ((bool)EvaluateExprNode(el.Condition, typeof(bool)))
            {
                return EvaluateExprNode(el.TrueNode, _targetType);
            }
            else
            {
                return EvaluateExprNode(el.FalseNode, _targetType);
            }
        }

        // Evaluate an identifier node
        object IAstExprNodeVisitor<object>.Visit(AstExprNodeIdentifier el)
        {
            if (_targetType == typeof(IdentifierLiteral))
            {
                return new IdentifierLiteral(el.Name);
            }

            // Is it a variable?
            if (_variables.TryGetValue(el.Name, out var varExpr))
            {
                try
                {
                    return EvaluateExprNode(varExpr, _targetType);
                }
                catch (Exception x)
                {
                    throw new CodeException($"Error resolving variable '{el.Name}'", x, el.Position);
                }
            }

            // Enum name?
            if (_targetType != null && _targetType.IsEnum)
            {
                try
                {
                    return Enum.Parse(_targetType, el.Name);
                }
                catch
                {
                    throw new CodeException($"{el.Name} is not a member of {_targetType.Name}", el.Position);
                }
            }

            // Unknown variable!
            throw new CodeException($"unknown identifier '{el.Name}'", el.Position);
        }

        // Evaluate a list node
        object IAstExprNodeVisitor<object>.Visit(AstExprNodeList el)
        {
            // Untyped list?
            if (_targetType == null)
            {
                return EvaluateAsListOfType(typeof(Object));
            }

            // Array?
            if (_targetType.IsArray && _targetType.GetArrayRank() == 1)
            {
                var list = EvaluateAsListOfType(_targetType.GetElementType());
                return list.GetType().GetMethod("ToArray").Invoke(list, null);
            }

            // List<T>
            if (_targetType.IsGenericType && 
                (_targetType.GetGenericTypeDefinition() == typeof(List<>) ||
                 _targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                var elType = _targetType.GetGenericArguments()[0];
                return EvaluateAsListOfType(elType);
            }

            // Implicit conversion
            var conversions = Context.GetImplicitConversions(_targetType);
            if (conversions != null)
            {
                conversions.ApplyImplicitArguments(ImplicitArgValues);
                conversions.RemoveContendersWithUnresolvedImplicitParameters();
                conversions.RemoveContendersByArgumentCount(1);

                foreach (var pType in conversions.GetParameterTypes(0))
                {
                    if (pType.IsGenericType && pType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        try
                        {
                            var list = EvaluateAsListOfType(pType.GetGenericArguments()[0]);
                            conversions.SetArgument(0, pType, list);
                        }
                        catch (Exception x)
                        {
                            conversions.SetArgumentError(0, pType, x);
                        }
                    }
                }

                return conversions.Invoke(el.Position);
            }

            throw new CodeException($"Type mismatch, can't convert list to `${_targetType.Name}`", el.Position);

            object EvaluateAsListOfType(Type elType)
            {
                var listType = typeof(List<>).MakeGenericType(elType);
                var list = Activator.CreateInstance(listType);
                foreach (var e in el.Elements)
                {
                    ((System.Collections.IList)list).Add(EvaluateExprNode(e, elType));
                }
                return list;
            }
        }

        // Evaluate a dictionary node
        object IAstExprNodeVisitor<object>.Visit(AstExprNodeDictionary el)
        {
            var conversions = Context.GetImplicitConversions(_targetType);
            if (conversions != null)
            {
                conversions.ApplyImplicitArguments(ImplicitArgValues);
                conversions.RemoveContendersWithUnresolvedImplicitParameters();
                conversions.RemoveContendersByArgumentCount(1);

                foreach (var pType in conversions.GetParameterTypes(0))
                {
                    if (pType.IsGenericType)
                    {
                        // IDictionary<,>
                        var genargs = pType.GetGenericArguments();
                        if (pType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                        {
                            try
                            {
                                var list = EvaluateAsDictionaryOfType(genargs[0], genargs[1]);
                                conversions.SetArgument(0, pType, list);
                            }
                            catch (Exception x)
                            {
                                conversions.SetArgumentError(0, pType, x);
                            }
                        }

                        // IList<KeyValuePair<,>>
                        if (pType.GetGenericTypeDefinition() == typeof(IList<>) &&
                            genargs[0].IsGenericType &&
                            genargs[0].GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                        {
                            var kvargs = genargs[0].GetGenericArguments();
                            try
                            {
                                var list = EvaluateAsListOfKeyValuePairs(kvargs[0], kvargs[1]);
                                conversions.SetArgument(0, pType, list);
                            }
                            catch (Exception x)
                            {
                                conversions.SetArgumentError(0, pType, x);
                            }
                        }
                    }
                }

                return conversions.Invoke(el.Position);
            }

            /*
            var dict = new Dictionary<string, object>();
            foreach (var e in el.Elements)
            {
                dict[e.Name] = EvaluateExprNode(e.Value);
            }
            return dict;
            */
            throw new NotImplementedException();

            object EvaluateAsDictionaryOfType(Type typeKey, Type typeValue)
            {
                var dictType = typeof(Dictionary<,>).MakeGenericType(typeKey, typeValue);
                var dict = Activator.CreateInstance(dictType); 
                foreach (var e in el.Elements)
                {
                    ((System.Collections.IDictionary)dict).Add(
                        EvaluateExprNode(e.Key, typeKey),
                        EvaluateExprNode(e.Value, typeValue)
                        );
                }
                return dict;
            }

            object EvaluateAsListOfKeyValuePairs(Type typeKey, Type typeValue)
            {
                var kvType = typeof(KeyValuePair<,>).MakeGenericType(typeKey, typeValue);
                var listType = typeof(List<>).MakeGenericType(kvType);
                var list = Activator.CreateInstance(listType);
                foreach (var e in el.Elements)
                {
                    var kv = Activator.CreateInstance(kvType,
                        EvaluateExprNode(e.Key, typeKey),
                        EvaluateExprNode(e.Value, typeValue)
                        );
                    ((System.Collections.IList)list).Add(kv);
                }
                return list;
            }
        }

        // Evaluate a function call node
        object IAstExprNodeVisitor<object>.Visit(AstExprNodeFunctionCall el)
        {
            // Get contenders
            var contenders = Context.GetFunctionsByName(el.Name);
            if (contenders == null)
                throw new CodeException($"Unknown method '{el.Name}'", el.Position);

            // Apply implicit arguments
            contenders.ApplyImplicitArguments(ImplicitArgValues);
            contenders.RemoveContendersWithUnresolvedImplicitParameters();

            // Resolve parameters
            contenders.RemoveContendersByArgumentCount(el.Arguments.Count);

            // Evaluate parameters
            for (int i = 0; i < el.Arguments.Count; i++)
            {
                foreach (var pt in contenders.GetParameterTypes(i))
                {
                    object value;
                    var exception = TryEvaluateExprNode(el.Arguments[i], pt, out value);
                    if (exception == null)
                        contenders.SetArgument(i, pt, value);
                    else
                        contenders.SetArgumentError(i, pt, exception);
                }
            }

            // Invoke it
            return contenders.Invoke(el.Position);
        }


    }
}


