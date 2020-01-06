using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IngameScript;

namespace JSONParserTests
{
	[TestClass]
	public class UnitTest3
	{
		[TestMethod]
		public void TestMethod1()
		{
			var source = "\"Hi There\"";
			var status = JSON.Parse(source).GetEnumerator();

			status.MoveNext();
			while (status.Current.HasWork())
			{
				Console.WriteLine($"{status.Current.charactersProcessed} / {source.Length - 1}");
				status.MoveNext();
			}
			Console.WriteLine($"{status.Current.charactersProcessed} / {source.Length - 1}");
			Assert.AreEqual(source.Length - 1, status.Current.charactersProcessed);
		}

		[TestMethod]
		public void TestMethod2()
		{
			var source = "\"Hi There\"";
			var status = JSON.Parse(source).GetEnumerator();

			while (status.MoveNext())
			{ }

			Assert.AreEqual(source.Length - 1, status.Current.charactersProcessed);
		}
	}
}
