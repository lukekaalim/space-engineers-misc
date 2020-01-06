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
	class WaypathManager
	{

		public readonly PanelCommander.CommandTree commandTree;
		readonly IMyTextSurface outputSurface;
		List<Waypath> paths;
		int currentIndex;

		public WaypathManager(IMyTextSurface outputSurface)
		{
			this.outputSurface = outputSurface;
			paths = new List<Waypath>();
			currentIndex = 0;

			commandTree = new PanelCommander.CommandTree()
			{
				label = "Waypath Manager",
				commands = new List<PanelCommander.Command>() {
					new PanelCommander.Command() { label = "Next Waypath", action = NextWaypath },
					new PanelCommander.Command() { label = "Delete Waypath", action = DeleteWaypath }
				},
				subtrees = new List<PanelCommander.CommandTree>(),
			};

			DrawOutput();
		}

		void DrawOutput()
		{
			if (outputSurface == null)
				return;
			if (currentIndex > paths.Count - 1)
			{
				outputSurface.WriteText(string.Join("\n",
					"Waypath Manager",
					"No Active Path",
					paths.Count
				));
				return;
			}
			var currentPath = paths[currentIndex];

			outputSurface.WriteText(string.Join("\n",
				"Waypath Manager",
				currentPath.name,
				(currentIndex + 1) + "/" + paths.Count
			));
		}

		public Waypath? GetCurrentWayPath()
		{
			if (currentIndex > paths.Count - 1)
			{
				return null;
			};
			return paths[currentIndex];
		}

		public void NextWaypath()
		{
			currentIndex = MathUtils.Mod(currentIndex + 1, paths.Count);
			DrawOutput();
		}

		public void AddWaypath(Waypath path)
		{
			paths.Add(path);
			DrawOutput();
		}

		public void DeleteWaypath()
		{
			if (paths.Count > 0)
			{
				paths.RemoveAt(paths.Count - 1);
				DrawOutput();
			}
		}

		public JSON.Element SaveState()
		{
			return JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
			{
				["paths"] = JSON.Element.NewArray(paths.Select(path => Waypath.ToJSON(path)).ToList())
			});
		}

		public void LoadState(JSON.Element rootElement)
		{
			paths = rootElement.jsonObject["paths"].jsonArray.Select(element => Waypath.FromJSON(element)).ToList();
			DrawOutput();
		}
	}
}
