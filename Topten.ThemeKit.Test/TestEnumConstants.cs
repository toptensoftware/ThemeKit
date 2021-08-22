using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xunit;

using Topten.ThemeKit;

namespace Topten.ThemeKit.Test
{
    public class EnumConstants
	{
		[Fact]
		public void EnumConstant()
		{
			string tkl =
				"class MyClass { fruitProp: Bananas }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(Fruit.Bananas, c.fruitProp);
		}

		[Fact]
		public void EnumFlagMath()
		{
			string tkl =
				"class MyClass { fontStyleProp: Bold|Italic }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(FontStyle.Bold | FontStyle.Italic, c.fontStyleProp);
		}
	}
}
