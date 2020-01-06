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
	partial class Program : MyGridProgram
	{
		static int Mod(int x, int m)
		{
			return (x % m + m) % m;
		}

		// This file contains your actual script.
		//
		// You can either keep all your code here, or you can create separate
		// code files to make your program easier to navigate while coding.
		//
		// In order to add a new utility class, right-click on your project, 
		// select 'New' then 'Add Item...'. Now find the 'Space Engineers'
		// category under 'Visual C# Items' on the left hand side, and select
		// 'Utility Class' in the main area. Name it in the box below, and
		// press OK. This utility class will be merged in with your code when
		// deploying your final script.
		//
		// You can also simply create a new utility class manually, you don't
		// have to use the template if you don't want to. Just do so the first
		// time to see what a utility class looks like.
		// 
		// Go to:
		// https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
		//
		// to learn more about ingame scripts.

		enum ProgramState
		{
			Uninitialized,
			Booted
		}

		ProgramState state = ProgramState.Uninitialized;

		public Program()
		{
			Runtime.UpdateFrequency = UpdateFrequency.Update100;
		}

		public void Save()
		{
			// Called when the program needs to save its state. Use
			// this method to save your state to the Storage field
			// or some other means. 
			// 
			// This method is optional and can be removed if not
			// needed.
		}

		ListLCDInterface lcd;
		IMyButtonPanel button;

		public void StartAssigning()
		{
			List<IMyTextPanel> textBlocks = new List<IMyTextPanel>();
			GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(textBlocks);

			var primaryLcd = textBlocks.Find(textBlock => textBlock.CustomName.StartsWith("[PRIMARY]"));
			if (primaryLcd == null)
			{
				Echo("Could not find primary LCD");
				return;
			}

			lcd = new ListLCDInterface(
				primaryLcd,
				"Assign Outputs",
				textBlocks.Select(textBlock => textBlock.CustomName).ToList()
			);

			List<IMyButtonPanel> buttonBlocks = new List<IMyButtonPanel>();
			GridTerminalSystem.GetBlocksOfType<IMyButtonPanel>(buttonBlocks);
			button = buttonBlocks.Find(buttonBlock => buttonBlock.CustomName.StartsWith("[PRIMARY_CONTROL]"));
			if (button == null)
			{
				Echo("Could not find primary Button control");
				return;
			}
		}

		struct EntityAndMeta<T> where T: IMyTerminalBlock
		{
			public readonly T entity;
			public readonly Dictionary<string, string> meta;

			public EntityAndMeta(T entity)
			{
				this.meta = NLquery.Parse(entity.CustomData);
				this.entity = entity;
			}
		}

		EngineeringDeck eng;

		void BootSystem()
		{
			var panels = new List<IMyTextPanel>();
			GridTerminalSystem.GetBlocksOfType(panels);

			var panelsPerDeck = new Dictionary<string, List<EntityAndMeta<IMyTextPanel>>>();
			foreach (var panel in panels)
			{
				Echo(panel.Font);
				var panelAndMeta = new EntityAndMeta<IMyTextPanel>(panel);
				string deckName;
				var deckResult = panelAndMeta.meta.TryGetValue("deck", out deckName);
				if (deckResult)
				{
					List<EntityAndMeta<IMyTextPanel>> panelListOfDeck;
					var listAlreadyExists = panelsPerDeck.TryGetValue(deckName, out panelListOfDeck);
					if (listAlreadyExists)
					{
						panelListOfDeck.Add(panelAndMeta);
					} else
					{
						panelsPerDeck.Add(deckName, new List<EntityAndMeta<IMyTextPanel>>(new [] { panelAndMeta }));
					}
				}
			}

			foreach(var deck in panelsPerDeck.Keys)
			{
				Echo(deck);
			}

			foreach(var deck in panelsPerDeck)
			{
				foreach(var panel in deck.Value)
				{
					panel.entity.WriteText(deck.Key);
				}
			}

			eng = new EngineeringDeck(panelsPerDeck["eng"]);

			state = ProgramState.Booted;
		}

		void HandleInput(string input)
		{
			var inputWords = input.Split(' ');
			if (inputWords.Length == 0)
				return;
			switch (inputWords[0])
			{
				case "eng":
				case "com":
					break;
				case "boot":
					BootSystem(); break;
				case "start":
					StartAssigning(); break;
				case "up":
					lcd.IncreaseIndex(); break;
				case "down":
					lcd.DecreaseIndex(); break;
			}
		}

		public void Main(string argument, UpdateType updateSource)
		{
			switch (updateSource)
			{
				case UpdateType.Update100:
					break;
				case UpdateType.Trigger:
				case UpdateType.Terminal:
					HandleInput(argument); break;
			}
		}
	}
}
