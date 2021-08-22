using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xunit;

using Topten.ThemeKit;

namespace Topten.ThemeKit.Test
{
    public class TestSimpleProperties
	{
		[Fact]
		public void StringProperty()
		{
			string tkl = "class MyClass { stringProp: \"Apples\" }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal("Apples", c.stringProp);
		}

		[Fact]
		public void IntProperty()
		{
			string tkl = "class MyClass { intProp: 23 }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(23, c.intProp);
		}

		[Fact]
		public void UIntProperty()
		{
			string tkl = "class MyClass { uintProp: 23 }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(23U, c.uintProp);
		}

		[Fact]
		public void UIntProperty_hex()
		{
			string tkl = "class MyClass { uintProp: 0x23 }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(0x23U, c.uintProp);
		}

		[Fact]
		public void DoubleProperty()
		{
			string tkl = "class MyClass { doubleProp: 23.45 }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(23.45, c.doubleProp);
		}

		[Fact]
		public void FloatProperty()
		{
			string tkl = "class MyClass { floatProp: 23.45 }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(23.45f, c.floatProp);
		}

		[Fact]
		public void BoolProperty()
		{
			string tkl = "class MyClass { boolProp: true }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.True(c.boolProp);
		}

		[Fact]
		public void ColorProperty()
		{
			string tkl = "class MyClass { colorProp: #112233}";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(0xFF112233U, c.colorProp.ARGB);
		}
	}
}
