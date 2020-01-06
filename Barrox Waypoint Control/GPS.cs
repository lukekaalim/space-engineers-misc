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
		static class GPS
		{
			public struct Position
			{
				public string name;
				public Vector3 vector;
				public Position(string name, Vector3 vector)
				{
					this.name = name;
					this.vector = vector;
				}
			}

			public static string Stringify(Position position)
			{
				var gpsComponents = new string[] { position.name, position.vector.X.ToString(), position.vector.Y.ToString(), position.vector.Z.ToString(), "" };
				return gpsComponents.Aggregate("GPS", (text, component) => text + ":" + component);
			}
			public static Position Parse(string source)
			{
				var gpsComponents = source.Split(':');
				if (gpsComponents.Length < 5 || gpsComponents[0] != "GPS")
					return new Position("Invalid Position", Vector3.Zero);
				
				return new Position(
					gpsComponents[1],
					new Vector3(float.Parse(gpsComponents[2]), float.Parse(gpsComponents[3]), float.Parse(gpsComponents[4]))
				);
			}
		}
	}
}
