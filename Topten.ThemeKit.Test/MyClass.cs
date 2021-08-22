using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topten.ThemeKit.Runtime;

namespace Topten.ThemeKit.Test
{
	public enum Fruit
	{
		Apples,
		Pears,
		Bananas,
	}

	[Flags]
	public enum FontStyle
	{
		None = 0,
		Bold = 0x0001,
		Italic = 0x0002,
		Underline = 0x0004,
	}

	public struct Vector
	{
		public int X;
		public int Y;
	}

	public class MyClass
	{
		public string stringProp { get; set; } = null;
		public int intProp { get; set; } = -1;
		public uint uintProp { get; set; } = 0;
		public double doubleProp { get; set; } = -1.0;
		public float floatProp { get; set; } = -1.0f;
		public bool boolProp { get; set; } = false;
		public ColorLiteral colorProp { get; set; }
		public int[] arrayInts { get; set; }
		public List<int> listInts { get; set; }
		public IEnumerable<int> enumerableInts { get; set; }
		public Fruit fruitProp { get; set; }
		public FontStyle fontStyleProp { get; set; }
		public Vector vectorProp { get; set; }
	}

	public static class HelperFunctions
	{
		public static int Scale(
			[ImplicitArgument("scalar")] int scalar, 
			int value)
		{
			return scalar * value;
		}

		[ImplicitConversion]
		public static int ToInt(string value)
		{
			return int.Parse(value);
		}

		public static int Add(int a, int b)
		{
			return a + b;
		}

		public static int Add(int a, int b, int c)
		{
			return a + b + c;
		}

		[ImplicitConversion]
		public static Vector list_int_to_vector(List<int> elements)
		{
			if (elements.Count != 2)
				throw new InvalidOperationException("Implicit conversion of list to vector requires two elements");

			return new Vector()
			{
				X = elements[0],
				Y = elements[1]
			};
		}

		[ImplicitConversion]
		public static Vector dictionary_to_vector(IList<KeyValuePair<IdentifierLiteral, int>> elements)
		{
			var vec = new Vector();
			foreach (var kv in elements)
			{
				if (kv.Key.Identifier == "X")
					vec.X = kv.Value;
				if (kv.Key.Identifier == "Y")
					vec.Y = kv.Value;
			}
			return vec;
		}
	}
}
