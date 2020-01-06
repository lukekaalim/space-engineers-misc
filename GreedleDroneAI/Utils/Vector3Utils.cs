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
	static class Vector3Utils
	{
		public static Vector3 Vector3FromJSON(JSON.Element rootElement)
		{
			return new Vector3(
				rootElement.jsonObject["x"].jsonNumber,
				rootElement.jsonObject["y"].jsonNumber,
				rootElement.jsonObject["z"].jsonNumber
			);
		}

		public static JSON.Element Vector3ToJSON(Vector3 vector)
		{
			return JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
			{
				["x"] = JSON.Element.NewNumber(vector.X),
				["y"] = JSON.Element.NewNumber(vector.Y),
				["z"] = JSON.Element.NewNumber(vector.Z),
			});
		}
	}
}
