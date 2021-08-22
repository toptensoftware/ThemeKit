using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xunit;

using Topten.ThemeKit;

namespace Topten.ThemeKit.Test
{
    public class TestArraysAndLists
	{
		[Fact]
		public void Array()
		{
			string tkl =
				"class MyClass { arrayInts: [1,2,3] }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(new int[] { 1, 2, 3 }, c.arrayInts);
		}

		[Fact]
		public void List()
		{
			string tkl =
				"class MyClass { listInts: [1,2,3] }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(new List<int>() { 1, 2, 3 }, c.listInts);
		}

		[Fact]
		public void Enumerable()
		{
			string tkl =
				"class MyClass { enumerableInts: [1,2,3] }";

			var t = new Theme();
			t.LoadString(tkl);

			var c = t.GetClass<MyClass>("MyClass");
			Assert.Equal(new List<int>() { 1, 2, 3 }, c.enumerableInts);
		}
	}
}
