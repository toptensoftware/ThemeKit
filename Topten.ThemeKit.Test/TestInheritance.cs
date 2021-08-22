using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xunit;

using Topten.ThemeKit;

namespace Topten.ThemeKit.Test
{
    public class TestInheritance
	{
		[Fact]
		public void SingleInheritance()
		{
			string tkl =
				"class BaseClass { intProp: 23 }\n" +
				"class MyClass : BaseClass { stringProp: \"Apples\" }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal("Apples", c.stringProp);
			Assert.Equal(23, c.intProp);
		}

		[Fact]
		public void MultipleInheritance()
		{
			string tkl =
				"class BaseClass1 { intProp: 23 }\n" +
				"class BaseClass2 { doubleProp: 24.56 }\n" +
				"class MyClass : BaseClass1, BaseClass2 { stringProp: \"Apples\" }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal("Apples", c.stringProp);
			Assert.Equal(23, c.intProp);
			Assert.Equal(24.56, c.doubleProp);
		}

		[Fact]
		public void MultipleInheritancePrecedence()
		{
			string tkl =
				"class BaseClass1 { intProp: 23, stringProp: \"Apples\" }\n" +
				"class BaseClass2 { intProp: 46 }\n" +
				"class MyClass : BaseClass1, BaseClass2 { stringProp: \"Apples\" }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal("Apples", c.stringProp);
			Assert.Equal(46, c.intProp);
		}
	}
}
