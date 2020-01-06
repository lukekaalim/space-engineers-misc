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
	partial class Program
	{
		public static class NLquery
		{
			public static Dictionary<string, string> Parse(string source)
			{
				var linesComponents = source.Split('\n').Select(line => line.Split('='));
				var result = new Dictionary<string, string>();
				foreach (string[] components in linesComponents)
				{
					if (components.Length > 1)
					{
						result.Add(components[0], components[1]);
					}
				}
				return result;
			}
			public static string Stringify(Dictionary<string, string> source)
			{
				return source.Aggregate("", (stringSoFar, kvp) => stringSoFar += '\n' + kvp.Key + '=' + kvp.Value);
			}
		}
	}
}
