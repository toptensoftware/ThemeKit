using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Xunit;

using Topten.ThemeKit;

namespace TestCases
{
    public class TestsGeneral
	{
		[Fact]
		public void FirstTest()
		{
			string str = "null";
			Assert.Equal("null", str);
		}
    }
}
