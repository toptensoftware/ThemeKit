using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Topten.ThemeKit.Runtime
{
    public enum OperatorType
    {
        // Binary
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulus,
        Compare,
        CompareLT,
        CompareLE,
        CompareGT,
        CompareGE,
        CompareEQ,
        CompareNE,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        LogicalAnd,
        LogicalOr,
        ShiftLeft,
        ShiftRight,

        // Unary
        Negate,
        BitwiseNot,
        LogicalNot,
    }

    public static class Operators
    {
        public static bool IsBinaryOp(OperatorType op)
        {
            return op >= OperatorType.Add && op <= OperatorType.ShiftRight;
        }

        public static bool IsUnaryOp(OperatorType op)
        {
            return op >= OperatorType.Negate && op <= OperatorType.LogicalNot;
        }

        // Add
        public static object Add(object a, object b)
        {
            switch (WiderType(a, b))
            {
                case TypeCode.Int32: return Convert.ToInt32(a) + Convert.ToInt32(b);
                case TypeCode.UInt32: return Convert.ToUInt32(a) + Convert.ToUInt32(b);
                case TypeCode.Int64: return Convert.ToInt64(a) + Convert.ToInt64(b);
                case TypeCode.UInt64: return Convert.ToUInt64(a) + Convert.ToUInt64(b);
                case TypeCode.Double: return Convert.ToDouble(a) + Convert.ToDouble(b);
                case TypeCode.Single: return Convert.ToSingle(a) + Convert.ToSingle(b);
                case TypeCode.Decimal: return Convert.ToDecimal(a) + Convert.ToDecimal(b);
                case TypeCode.String: return Convert.ToString(a) + Convert.ToString(b);
            }
            unsupported(a, b, "add");
            return null;
        }

        // Subtract
        public static object Subtract(object a, object b)
        {
            switch (WiderType(a, b))
            {
                case TypeCode.Int32: return Convert.ToInt32(a) - Convert.ToInt32(b);
                case TypeCode.UInt32: return Convert.ToUInt32(a) - Convert.ToUInt32(b);
                case TypeCode.Int64: return Convert.ToInt64(a) - Convert.ToInt64(b);
                case TypeCode.UInt64: return Convert.ToUInt64(a) - Convert.ToUInt64(b);
                case TypeCode.Double: return Convert.ToDouble(a) - Convert.ToDouble(b);
                case TypeCode.Single: return Convert.ToSingle(a) - Convert.ToSingle(b);
                case TypeCode.Decimal: return Convert.ToDecimal(a) - Convert.ToDecimal(b);
            }

            unsupported(a, b, "subtract");
            return null;
        }

        // Multiply
        public static object Multiply(object a, object b)
        {
            switch (WiderType(a, b))
            {
                case TypeCode.Int32: return Convert.ToInt32(a) * Convert.ToInt32(b);
                case TypeCode.UInt32: return Convert.ToUInt32(a) * Convert.ToUInt32(b);
                case TypeCode.Int64: return Convert.ToInt64(a) * Convert.ToInt64(b);
                case TypeCode.UInt64: return Convert.ToUInt64(a) * Convert.ToUInt64(b);
                case TypeCode.Double: return Convert.ToDouble(a) * Convert.ToDouble(b);
                case TypeCode.Single: return Convert.ToSingle(a) * Convert.ToSingle(b);
                case TypeCode.Decimal: return Convert.ToDecimal(a) * Convert.ToDecimal(b);
            }

            unsupported(a, b, "multiply");
            return null;
        }

        // Divide
        public static object Divide(object a, object b)
        {
            switch (WiderType(a, b))
            {
                case TypeCode.Int32: return Convert.ToInt32(a) / Convert.ToInt32(b);
                case TypeCode.UInt32: return Convert.ToUInt32(a) / Convert.ToUInt32(b);
                case TypeCode.Int64: return Convert.ToInt64(a) / Convert.ToInt64(b);
                case TypeCode.UInt64: return Convert.ToUInt64(a) / Convert.ToUInt64(b);
                case TypeCode.Double: return Convert.ToDouble(a) / Convert.ToDouble(b);
                case TypeCode.Single: return Convert.ToSingle(a) / Convert.ToSingle(b);
                case TypeCode.Decimal: return Convert.ToDecimal(a) / Convert.ToDecimal(b);
            }

            unsupported(a, b, "divide");
            return null;
        }

        // Modulus
        public static object Modulus(object a, object b)
        {
            switch (WiderType(a, b))
            {
                case TypeCode.Int32: return Convert.ToInt32(a) % Convert.ToInt32(b);
                case TypeCode.UInt32: return Convert.ToUInt32(a) % Convert.ToUInt32(b);
                case TypeCode.Int64: return Convert.ToInt64(a) % Convert.ToInt64(b);
                case TypeCode.UInt64: return Convert.ToUInt64(a) % Convert.ToUInt64(b);
                case TypeCode.Double: return Convert.ToDouble(a) % Convert.ToDouble(b);
                case TypeCode.Single: return Convert.ToSingle(a) % Convert.ToSingle(b);
                case TypeCode.Decimal: return Convert.ToDecimal(a) % Convert.ToDecimal(b);
            }

            unsupported(a, b, "modulus");
            return null;
        }

        // Negate
        public static object Negate(object a)
        {
            switch (WiderType(a, a))
            {
                case TypeCode.Int32: return -Convert.ToInt32(a);
                case TypeCode.UInt32: return -Convert.ToUInt32(a);
                case TypeCode.Int64: return -Convert.ToInt64(a);
                case TypeCode.Double: return -Convert.ToDouble(a);
                case TypeCode.Single: return -Convert.ToSingle(a);
                case TypeCode.Decimal: return -Convert.ToDecimal(a);
            }

            unsupported(a, "negate");
            return null;
        }

        // Compare
        public static object Compare(object a, object b)
        {
            switch (WiderType(a, b))
            {
                case TypeCode.Int32: return Convert.ToInt32(a).CompareTo(Convert.ToInt32(b));
                case TypeCode.UInt32: return Convert.ToUInt32(a).CompareTo(Convert.ToUInt32(b));
                case TypeCode.Int64: return Convert.ToInt64(a).CompareTo(Convert.ToInt64(b));
                case TypeCode.UInt64: return Convert.ToUInt64(a).CompareTo(Convert.ToUInt64(b));
                case TypeCode.Double: return Convert.ToDouble(a).CompareTo(Convert.ToDouble(b));
                case TypeCode.Single: return Convert.ToSingle(a).CompareTo(Convert.ToSingle(b));
                case TypeCode.Decimal: return Convert.ToDecimal(a).CompareTo(Convert.ToDecimal(b));
                case TypeCode.Boolean: return Convert.ToBoolean(a).CompareTo(Convert.ToBoolean(b));
                case TypeCode.String: return Convert.ToString(a).CompareTo(Convert.ToString(b));
            }

            unsupported(a, b, "compare");
            return null;
        }

        // Compare LT
        public static object CompareLT(object a, object b)
        {
            return ((int)Compare(a, b)) < 0;
        }

        // Compare LE
        public static object CompareLE(object a, object b)
        {
            return ((int)Compare(a, b)) <= 0;
        }

        // Compare GT
        public static object CompareGT(object a, object b)
        {
            return ((int)Compare(a, b)) > 0;
        }

        // Compare GE
        public static object CompareGE(object a, object b)
        {
            return ((int)Compare(a, b)) >= 0;
        }

        // Compare EQ
        public static object CompareEQ(object a, object b)
        {
            return ((int)Compare(a, b)) == 0;
        }

        // Compare NE
        public static object CompareNE(object a, object b)
        {
            return ((int)Compare(a, b)) != 0;
        }


        // Bitwise And
        public static object BitwiseAnd(object a, object b)
        {
            switch (WiderType(a, b))
            {
                case TypeCode.Int32: return Convert.ToInt32(a) & Convert.ToInt32(b);
                case TypeCode.UInt32: return Convert.ToUInt32(a) & Convert.ToUInt32(b);
                case TypeCode.Int64: return Convert.ToInt64(a) & Convert.ToInt64(b);
                case TypeCode.UInt64: return Convert.ToUInt64(a) & Convert.ToUInt64(b);
            }

            unsupported(a, b, "bitwise and");
            return null;
        }

        // Bitwize Or
        public static object BitwiseOr(object a, object b)
        {
            switch (WiderType(a, b))
            {
                case TypeCode.Int32: return Convert.ToInt32(a) | Convert.ToInt32(b);
                case TypeCode.UInt32: return Convert.ToUInt32(a) | Convert.ToUInt32(b);
                case TypeCode.Int64: return Convert.ToInt64(a) | Convert.ToInt64(b);
                case TypeCode.UInt64: return Convert.ToUInt64(a) | Convert.ToUInt64(b);
            }

            unsupported(a, b, "bitwise or");
            return null;
        }

        // Bitwise Xor
        public static object BitwiseXor(object a, object b)
        {
            switch (WiderType(a, b))
            {
                case TypeCode.Int32: return Convert.ToInt32(a) ^ Convert.ToInt32(b);
                case TypeCode.UInt32: return Convert.ToUInt32(a) ^ Convert.ToUInt32(b);
                case TypeCode.Int64: return Convert.ToInt64(a) ^ Convert.ToInt64(b);
                case TypeCode.UInt64: return Convert.ToUInt64(a) ^ Convert.ToUInt64(b);
            }

            unsupported(a, b, "bitwise xor");
            return null;
        }

        // Bitwise Not
        public static object BitwiseNot(object a)
        {
            switch (WiderType(a, a))
            {
                case TypeCode.Int32: return ~Convert.ToInt32(a);
                case TypeCode.UInt32: return ~Convert.ToUInt32(a);
                case TypeCode.Int64: return ~Convert.ToInt64(a);
                case TypeCode.UInt64: return ~Convert.ToUInt64(a);
            }

            unsupported(a, "bitwise not");
            return null;
        }

        // Logical And
        public static object LogicalAnd(object a, object b)
        {
            switch (WiderType(a, b))
            {
                case TypeCode.Boolean: return Convert.ToBoolean(a) && Convert.ToBoolean(b);
            }

            unsupported(a, b, "logical and");
            return null;
        }

        // Logical Or
        public static object LogicalOr(object a, object b)
        {
            switch (WiderType(a, b))
            {
                case TypeCode.Boolean: return Convert.ToBoolean(a) || Convert.ToBoolean(b);
            }

            unsupported(a, b, "logical or");
            return null;
        }

        // Logical Not
        public static object LogicalNot(object a)
        {
            switch (WiderType(a, a))
            {
                case TypeCode.Boolean: return !Convert.ToBoolean(a);
            }

            unsupported(a, "logical not");
            return null;
        }

        // Shift Left
        public static object ShiftLeft(object a, object b)
        {
            if (WiderType(b, b) == TypeCode.Int32)
            {
                switch (WiderType(a, a))
                {
                    case TypeCode.Int32: return Convert.ToInt32(a) << Convert.ToInt32(b);
                    case TypeCode.UInt32: return Convert.ToUInt32(a) << Convert.ToInt32(b);
                    case TypeCode.Int64: return Convert.ToInt64(a) << Convert.ToInt32(b);
                    case TypeCode.UInt64: return Convert.ToUInt64(a) << Convert.ToInt32(b);
                }
            }

            unsupported(a, b, "shl");
            return null;
        }

        // Shift Right
        public static object ShiftRight(object a, object b)
        {
            if (WiderType(b, b) == TypeCode.Int32)
            {
                switch (WiderType(a, a))
                {
                    case TypeCode.Int32: return Convert.ToInt32(a) >> Convert.ToInt32(b);
                    case TypeCode.UInt32: return Convert.ToUInt32(a) >> Convert.ToInt32(b);
                    case TypeCode.Int64: return Convert.ToInt64(a) >> Convert.ToInt32(b);
                    case TypeCode.UInt64: return Convert.ToUInt64(a) >> Convert.ToInt32(b);
                }
            }

            unsupported(a, b, "shl");
            return null;
        }

        // Arbitrary binary operation
        public static object Evaluate(OperatorType op, object a, object b)
        {
            if (op >= OperatorType.Add && op <= OperatorType.ShiftRight)
            {
                return binary_operations[(int)op](a, b);
            }
            throw new ArgumentException();
        }

        // Arbitrary unary operation
        public static object Evaluate(OperatorType op, object a)
        {
            if (op >= OperatorType.Negate && op <= OperatorType.LogicalNot)
            {
                return unary_operations[(int)(op - OperatorType.Negate)](a);
            }
            throw new ArgumentException();
        }

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
        /// Tries to implicitly convert a type, thowing an exception
        /// if it doesn't fit in the target type, or there's loss 
        /// of precision when casting from floating point types
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType">The target type</param>
        /// <returns></returns>
        public static object ImplicitChangeType(object value, Type targetType)
        {
            if (value == null && !targetType.IsValueType)
                return null;

            // If converting floating point to non-floating point
            // then check the conversion is reversible
            // NB: We allow double to float assignment even if loss of precision
            var sourceType = value.GetType();
            if ((sourceType == typeof(double) || sourceType == typeof(float)) &&
                 (targetType != typeof(double) && targetType != typeof(float)))
            {
                var targetValue = Convert.ChangeType(value, targetType);
                var testValue = Convert.ChangeType(targetValue, value.GetType());
                if (Object.Equals(value, testValue))
                    return targetValue;
                else
                    throw new OverflowException($"Unable to convert '{sourceType}' to '{targetType}' without loss of precision");
            }

            return Convert.ChangeType(value, targetType);
        }

        /// <summary>
        /// Tries to implicitly convert a type, thowing an exception
        /// if it doesn't fit in the target type, or there's loss 
        /// of precision when casting from floating point types
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType">The target type</param>
        /// <returns></returns>
        public static object ExplicitChangeType(object value, Type targetType)
        {
            if (value == null && !targetType.IsValueType)
                return null;

            if (targetType.IsEnum)
            {
                return Enum.ToObject(targetType, value);
            }

            return Convert.ChangeType(value, targetType);
        }

        static Func<object, object, object>[] binary_operations = new Func<object, object, object>[]
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulus,
            Compare,
            CompareLT,
            CompareLE,
            CompareGT,
            CompareGE,
            CompareEQ,
            CompareNE,
            BitwiseAnd,
            BitwiseOr,
            BitwiseXor,
            LogicalAnd,
            LogicalOr,
            ShiftLeft,
            ShiftRight,
        };

        static Func<object, object>[] unary_operations = new Func<object, object>[]
        {
            Negate,
            BitwiseNot,
            LogicalNot,
        };

        static void unsupported(object a, object b, string op)
        {
            throw new System.InvalidCastException(string.Format("Can't {0} objects of type {1} and {2}", op, a.GetType(), b.GetType()));
        }

        static void unsupported(object a, string op)
        {
            throw new System.InvalidCastException(string.Format("Can't {0} object of type {1}", op, a.GetType()));
        }

        static TypeCode WiderType(object a, object b)
        {
            TypeCode ta = Type.GetTypeCode(a.GetType());
            TypeCode tb = Type.GetTypeCode(b.GetType());
            return type_map[(int)ta, (int)tb];
        }


        #region Binary Operation Type Map
        // This horrible map tells us what two operands should be converted to for a binary operation.
        static TypeCode[,] type_map = new TypeCode[19, 19]
        {
//		  a:	Empty,				Object,				DBNull,				Boolean,			Char,				SByte,				Byte,				Int16,				UInt16,				Int32,				UInt32,				Int64,				UInt64,				Single,				Double,				Decimal,			DateTime,			Missing				String
// b:
/*Empty*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty },
/*Object*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*DBNull*/  {   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*Boolean*/ {   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Boolean,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
																																																																																									
/*Char*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Int32,     TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int64,     TypeCode.UInt64,    TypeCode.Single,    TypeCode.Double,    TypeCode.Decimal,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*SByte*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Int32,     TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int64,     TypeCode.UInt64,    TypeCode.Single,    TypeCode.Double,    TypeCode.Decimal,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*Byte*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.Int64,     TypeCode.UInt64,    TypeCode.Single,    TypeCode.Double,    TypeCode.Decimal,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*Int16*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Int32,     TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int64,     TypeCode.UInt64,    TypeCode.Single,    TypeCode.Double,    TypeCode.Decimal,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*UInt16*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.Int64,     TypeCode.UInt64,    TypeCode.Single,    TypeCode.Double,    TypeCode.Decimal,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*Int32*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Int32,     TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int32,     TypeCode.UInt32,    TypeCode.Int64,     TypeCode.UInt64,    TypeCode.Single,    TypeCode.Double,    TypeCode.Decimal,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*UInt32*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.UInt32,    TypeCode.Int64,     TypeCode.UInt64,    TypeCode.Single,    TypeCode.Double,    TypeCode.Decimal,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*Int64*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Int64,     TypeCode.Int64,     TypeCode.UInt64,    TypeCode.Int64,     TypeCode.UInt64,    TypeCode.Int64,     TypeCode.UInt64,    TypeCode.Int64,     TypeCode.Empty,     TypeCode.Single,    TypeCode.Double,    TypeCode.Decimal,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*UInt64*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.UInt64,    TypeCode.UInt64,    TypeCode.UInt64,    TypeCode.UInt64,    TypeCode.UInt64,    TypeCode.UInt64,    TypeCode.UInt64,    TypeCode.Empty,     TypeCode.UInt64,    TypeCode.Single,    TypeCode.Double,    TypeCode.Decimal,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
																																																																																									
/*Single*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Single,    TypeCode.Single,    TypeCode.Single,    TypeCode.Single,    TypeCode.Single,    TypeCode.Single,    TypeCode.Single,    TypeCode.Single,    TypeCode.Single,    TypeCode.Single,    TypeCode.Double,    TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*Double*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Double,    TypeCode.Double,    TypeCode.Double,    TypeCode.Double,    TypeCode.Double,    TypeCode.Double,    TypeCode.Double,    TypeCode.Double,    TypeCode.Double,    TypeCode.Double,    TypeCode.Double,    TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
																																																																																									
/*Decimal*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Decimal,   TypeCode.Decimal,   TypeCode.Decimal,   TypeCode.Decimal,   TypeCode.Decimal,   TypeCode.Decimal,   TypeCode.Decimal,   TypeCode.Decimal,   TypeCode.Decimal,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Decimal,   TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
																																																																																									
/*DateTime*/{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*Missing*/	{   TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.Empty,     TypeCode.String },
/*String*/	{   TypeCode.Empty,     TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String,    TypeCode.String },
        };
        #endregion

    }

}
