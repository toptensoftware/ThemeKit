using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xunit;

using Topten.ThemeKit;

namespace Topten.ThemeKit.Test
{
    public class TestFuntionCalls
	{
		[Fact]
		public void SimpleCall()
		{
			string tkl =
				"class MyClass { intProp: Add(10, 20, 30) }";

			var t = new Theme();
			t.LoadString(tkl);
			t.ResolverContext.AddStaticMethods(typeof(HelperFunctions));

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(60, c.intProp);
		}

		[Fact]
		public void CallWithImplicitArgs()
		{
			string tkl =
				"class MyClass { intProp: Scale(23) }";

			var t = new Theme();
			t.LoadString(tkl);
			t.ResolverContext.AddStaticMethods(typeof(HelperFunctions));
			t.ImplicitArgumentValues["scalar"] = 2;

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(46, c.intProp);
		}
	}
}
