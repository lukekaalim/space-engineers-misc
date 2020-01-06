using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
	class Ticker
	{
		List<Action> onTicks = new List<Action>();

		public void Tick() {
			foreach (var tick in onTicks.ToList())
				tick();
		}

		public class Promise<T>
		{
			Action<T> then;
			Ticker ticker;
			IEnumerator<T> enumerator;
			int tickId;
			int enumerationsPerTick;

			public Promise(IEnumerator<T> enumerator, Ticker ticker, Action<T> then, int enumerationsPerTick = 10)
			{
				this.then = then;
				this.ticker = ticker;
				this.enumerator = enumerator;
				this.enumerationsPerTick = enumerationsPerTick;

				this.tickId = ticker.StartListeningForTicks(Tick);
			}

			void Tick()
			{
				for (int i = 0; i < enumerationsPerTick; i++)
				{
					var moreValues = enumerator.MoveNext();
					if (!moreValues)
					{
						ticker.StopListeningForTicks(tickId);
						then(enumerator.Current);
						enumerator.Dispose();
						break;
					}
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

		public Promise<T> CreatePromise<T>(IEnumerator<T> enumerator, Action<T> then, int enumerationsPerTick = 10)
		{
			return new Promise<T>(enumerator, this, then, enumerationsPerTick);
		}
	}
}
