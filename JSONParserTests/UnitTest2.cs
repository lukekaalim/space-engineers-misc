using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IngameScript;
using System.Collections.Generic;

namespace JSONParserTests
{
	[TestClass]
	public class UnitTest2
	{
		[TestMethod]
		public void TestStringSerialization()
		{
			Assert.AreEqual(JSON.Stringify(JSON.Element.NewString("Hello")), "\"Hello\"");
		}

		[TestMethod]
		public void TestNumberSerialization()
		{
			Assert.AreEqual(JSON.Stringify(JSON.Element.NewNumber(123)), "123");
			Assert.AreEqual(JSON.Stringify(JSON.Element.NewNumber(0.5)), "0.5");
		}

		[TestMethod]
		public void TestNullSerialization()
		{
			Assert.AreEqual(JSON.Stringify(JSON.Element.NewNull()), "null");
		}

		[TestMethod]
		public void TestBooleanSerialization() {
			Assert.AreEqual(JSON.Stringify(JSON.Element.NewBoolean(true)), "true");
			Assert.AreEqual(JSON.Stringify(JSON.Element.NewBoolean(false)), "false");
		}

		[TestMethod]
		public void TestArraySerialization()
		{
			Assert.AreEqual(JSON.Stringify(JSON.Element.NewArray(new List<JSON.Element>(new JSON.Element[] { JSON.Element.NewNumber(5) }))), "[5]");
		}

		[TestMethod]
		public void TestArraySerializationOrder()
		{
			Assert.AreEqual(
				JSON.Stringify(JSON.Element.NewArray(new List<JSON.Element>(new JSON.Element[] {
					JSON.Element.NewNumber(10),
					JSON.Element.NewNumber(15),
					JSON.Element.NewNumber(20),
					JSON.Element.NewNumber(30),
				}))),
				"[10,15,20,30]"
			);
		}

		[TestMethod]
		public void TestObjectSerialization()
		{
			Assert.AreEqual(JSON.Stringify(JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
			{
				["hello"] = JSON.Element.NewString("World")
			})), "{\"hello\":\"World\"}");
		}
	}
}
