using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IngameScript;
using System.Collections.Generic;
using System.Linq;

namespace JSONParserTests
{
	class Ticker
	{
		List<Action> onTicks = new List<Action>();

		public void Tick()
		{
			foreach (var tick in onTicks.ToList())
				tick();
		}

		public class Promise<T>
		{
			Action<T> then;
			Ticker ticker;
			IEnumerator<T> enumerator;
			int tickId;

			public Promise(IEnumerator<T> enumerator, Ticker ticker, Action<T> then)
			{
				this.then = then;
				this.ticker = ticker;
				this.enumerator = enumerator;

				this.tickId = ticker.StartListeningForTicks(Tick);
			}

			void Tick()
			{
				var done = enumerator.MoveNext();
				if (done)
				{
					ticker.StopListeningForTicks(tickId);
					then(enumerator.Current);
					enumerator.Dispose();
				}
			}
		}

		public int StartListeningForTicks(Action listener)
		{
			onTicks.Add(listener);
			return onTicks.Count - 1;
		}

		public void StopListeningForTicks(int listenerId)
		{
			onTicks.RemoveAt(listenerId);
		}

		public Promise<T> CreatePromise<T>(IEnumerator<T> enumerator, Action<T> then)
		{
			return new Promise<T>(enumerator, this, then);
		}
	}


	[TestClass]
	public class UnitTest4
	{
		[TestMethod]
		public void TestMethod1()
		{
			Ticker ticker = new Ticker();
			string source = "null";
			ticker.CreatePromise(JSON.Parse(source).GetEnumerator(), status => Console.Write("Are we done?", status.state.ToString()));
			for (int i = 0; i < 1; i++)
			{
				ticker.Tick();
			}
		}
	}
}
