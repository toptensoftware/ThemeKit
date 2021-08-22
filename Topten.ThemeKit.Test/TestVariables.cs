using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xunit;

using Topten.ThemeKit;

namespace Topten.ThemeKit.Test
{
    public class TestVariables
	{
		[Fact]
		public void SimpleVariable()
		{
			string tkl =
				"var myVar = 23;\n" +
				"class MyClass { intProp: myVar }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(23, c.intProp);
		}
	}
}
