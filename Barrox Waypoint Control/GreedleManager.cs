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
	class GreedleManager
	{
		public readonly PanelCommander.CommandTree commandTree;
		struct Greedle
		{
			public string name;
			public Vector3 position;
		}

		int calledList = 0;
		int checkForUpdates = 0;
		int greedleResponsesCollected = 0;
		IMyIntergridCommunicationSystem IGC;
		List<Greedle> greedles;
		IMyBroadcastListener responseListener;
		IMyTextSurface outputSurface;

		public GreedleManager(IMyIntergridCommunicationSystem IGC, IMyTextSurface outputSurface)
		{
			this.IGC = IGC;
			this.responseListener = IGC.RegisterBroadcastListener("greedle_response");
			this.responseListener.SetMessageCallback("Message Callback");
			this.greedles = new List<Greedle>();
			this.outputSurface = outputSurface;

			commandTree = new PanelCommander.CommandTree()
			{
				label = "Greedle Commands",
				commands = new List<PanelCommander.Command>() {
					new PanelCommander.Command() { label = "List", action = ListGreedles },
					new PanelCommander.Command() { label = "Check For Greedle Updates", action = CheckForUpdates }
				},
				subtrees = new List<PanelCommander.CommandTree>(),
			};
			ListGreedles();
		}

		void CheckForUpdates()
		{
			checkForUpdates++;
			while (responseListener.HasPendingMessage)
			{
				greedleResponsesCollected++;
				var response = GreedleResponse.FromJSON(JSON.SyncParse(responseListener.AcceptMessage().As<string>()));
				greedles.Add(new Greedle() { name = response.name, position = response.position });
			}
			DrawOutput();
		}

		void DrawOutput()
		{
			if (outputSurface == null)
				return;
			outputSurface.ContentType = ContentType.TEXT_AND_IMAGE;

			string greedleListText = string.Join("\n", greedles
				.Select(greedle => $"{greedle.name} {Vector3Utils.ToShorthand(greedle.position)}"));

			outputSurface.WriteText(string.Join("\n",
				"Greedles",
				"Called List " + calledList + " times",
				"Called CheckForUpdates " + checkForUpdates + " times",
				"Recieved a Greedle Response " + greedleResponsesCollected + " times",
				greedles.Count,
				greedleListText
				)
			);
		}

		void ListGreedles()
		{
			calledList++;
			greedles.Clear();
			IGC.SendBroadcastMessage("greedle_directive", JSON.Stringify(GreedleDirective.ToJSON(new GreedleDirective()
			{
				type = GreedleDirective.DirectiveType.Respond
			})));
			DrawOutput();
		}

		void SendGreedle(string name, Vector3 target)
		{
			IGC.SendBroadcastMessage("greedle_directive", JSON.Stringify(GreedleDirective.ToJSON(new GreedleDirective()
			{
				type = GreedleDirective.DirectiveType.Target,
				targetDirective = new GreedleDirective.TargetDirective()
				{
					addressedTo = name,
					target = target,
				},
			})));
		}

		public int SendGreedles(Vector3[] targets)
		{
			for (int i = 0; i < greedles.Count && i < targets.Length; i++)
			{
				SendGreedle(greedles[i].name, targets[i]);
			}
			return Math.Min(greedles.Count, targets.Length);
		}

		public void Tick()
		{
			CheckForUpdates();
		}
	}
}
