using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xunit;

using Topten.ThemeKit;

namespace Topten.ThemeKit.Test
{
    public class TestOperators
	{
		[Fact]
		public void Inplace()
		{
			string tkl =
				"class MyClass { intProp: 23 + 10, }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(33, c.intProp);
		}
	}
}
