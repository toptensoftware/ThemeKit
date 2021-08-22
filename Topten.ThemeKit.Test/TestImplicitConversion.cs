using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xunit;

using Topten.ThemeKit;

namespace Topten.ThemeKit.Test
{
    public class TestImplicitConversions
	{
		[Fact]
		public void ArrayToStruct()
		{
			string tkl =
				"class MyClass { vectorProp: [ 23, 24 ] }";

			var t = new Theme();
			t.LoadString(tkl);
			t.ResolverContext.AddStaticMethods(typeof(HelperFunctions));

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(23, c.vectorProp.X);
			Assert.Equal(24, c.vectorProp.Y);
		}

		[Fact]
		public void DictionaryToStruct()
		{
			string tkl =
				"class MyClass { vectorProp: { X:23, Y:24 } }";

			var t = new Theme();
			t.LoadString(tkl);
			t.ResolverContext.AddStaticMethods(typeof(HelperFunctions));

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(23, c.vectorProp.X);
			Assert.Equal(24, c.vectorProp.Y);
		}

		[Fact]
		public void StringToInt()
		{
			string tkl =
				"class MyClass { intProp: \"23\" }";

			var t = new Theme();
			t.LoadString(tkl);
			t.ResolverContext.AddStaticMethods(typeof(HelperFunctions));

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(23, c.intProp);
		}
	}
}
