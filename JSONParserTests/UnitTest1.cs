using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IngameScript;

namespace JSONParserTests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestString()
		{
			var result = JSON.SyncParse("\"Hi There\"");
			Assert.AreEqual(result.type, JSON.ElementType.JSONString);
			Assert.AreEqual(result.jsonString, "Hi There");
		}

		[TestMethod]
		public void TestObject()
		{
			var result = JSON.SyncParse("{\"Hi There\": \"Gorgeous\"}");
			Assert.AreEqual(result.type, JSON.ElementType.JSONObject);
			Assert.AreEqual(result.jsonObject["Hi There"].jsonString, "Gorgeous");
		}

		[TestMethod]
		public void TestNesting()
		{
			var result = JSON.SyncParse("{\"Hi There\": { \"adoring\": \"fans\"} }");
			Assert.AreEqual(result.type, JSON.ElementType.JSONObject);
			Assert.AreEqual(result.jsonObject["Hi There"].jsonObject["adoring"].jsonString, "fans");
		}

		[TestMethod]
		public void TestTrueKeyword()
		{
			var result = JSON.SyncParse("true");
			Assert.AreEqual(result.type, JSON.ElementType.JSONBoolean);
			Assert.AreEqual(result.jsonBoolean, true);
		}

		[TestMethod]
		public void TestFalseKeyword()
		{
			var result = JSON.SyncParse("false");
			Assert.AreEqual(result.type, JSON.ElementType.JSONBoolean);
			Assert.AreEqual(result.jsonBoolean, false);
		}

		[TestMethod]
		public void TestNullKeyword()
		{
			var result = JSON.SyncParse("null");
			Assert.AreEqual(result.type, JSON.ElementType.JSONNull);
		}

		[TestMethod]
		public void TestNumber()
		{
			Assert.AreEqual(JSON.SyncParse("12345").jsonNumber, 12345d);
			Assert.AreEqual(JSON.SyncParse("-12345").jsonNumber, -12345d);
			Assert.AreEqual(JSON.SyncParse("-500.0").jsonNumber, -500.0);
		}

		[TestMethod]
		public void TestArray()
		{
			var result = JSON.SyncParse("[1,2,3,4]");
			Assert.AreEqual(result.type, JSON.ElementType.JSONArray);
			Assert.AreEqual(result.jsonArray[0].jsonNumber, 1);
			Assert.AreEqual(result.jsonArray[1].jsonNumber, 2);
			Assert.AreEqual(result.jsonArray[2].jsonNumber, 3);
			Assert.AreEqual(result.jsonArray[3].jsonNumber, 4);
		}

		[TestMethod]
		public void TestComplex()
		{
			JSON.SyncParse("[{}, { \"I'm ther sherrif\": \" howdy owdy howdy\" }, 222.2]");
		}
		[TestMethod]
		public void TextExample()
		{
			JSON.SyncParse("{\"shouldDisplayBarroxOutput\": true}");
		}
		[TestMethod]
		public void TextMultiproperty()
		{
			JSON.SyncParse("{\"currentPathIndex\": 0,\"paths\": [{\"name\": \"Up The Mountain\",\"points\": [{\"name\": \"First Point\",\"radius\": 10,\"position\": {\"x\": -49306.05859375,\"y\": -22156.3984375,\"z\": -29263.84375}}]}]}");
		}

		[TestMethod]
		public void TestEmpty()
		{
			Assert.ThrowsException<Exception>(() => JSON.SyncParse(""));
		}
	}
}
