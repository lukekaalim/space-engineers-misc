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
	class Location
	{
		public Dictionary<string, Waypath> pathsToLocations;
		public string name;
		public Vector3 position;

		public static Location FromJSON(JSON.Element rootElement)
		{
			return new Location()
			{
				name = rootElement.jsonObject["name"].jsonString,
				position = Vector3Utils.Vector3FromJSON(rootElement.jsonObject["position"]),
				pathsToLocations = rootElement.jsonObject["paths"].jsonObject
					.Select(element => new KeyValuePair<string, Waypath>(element.Key, Waypath.FromJSON(element.Value)))
					.ToDictionary(entry => entry.Key, entry => entry.Value)
			};
		}

		public static JSON.Element ToJSON(Location location)
		{
			return JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
			{
				["name"] = JSON.Element.NewString(location.name),
				["position"] = Vector3Utils.Vector3ToJSON(location.position),
				["pathsToLocations"] = JSON.Element.NewObject(location.pathsToLocations
					.Select(pathToLocation => new KeyValuePair<string, JSON.Element>(pathToLocation.Key, Waypath.ToJSON(pathToLocation.Value)))
					.ToDictionary(entry => entry.Key, entry => entry.Value)),
			});
		}
	}
}
