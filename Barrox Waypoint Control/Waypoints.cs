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
	struct Waypoint
	{
		public string name;

		public float angularDeviation;
		public float radius;

		public Vector3 position;
		public Vector3 direction;

		public Waypoint(string name, float radius, Vector3 position, Vector3 direction = new Vector3())
		{
			this.name = name;
			this.position = position;
			this.direction = direction;

			this.radius = radius;
			this.angularDeviation = 0.5f;
		}

		public static Waypoint FromJSON(JSON.Element rootElement)
		{
			string name = rootElement.jsonObject["name"].jsonString;
			float radius = (float)rootElement.jsonObject["radius"].jsonNumber;

			Vector3 position = Vector3Utils.Vector3FromJSON(rootElement.jsonObject["position"]);

			Vector3 direction = Vector3.Zero;
			JSON.Element directionElement;
			if (rootElement.jsonObject.TryGetValue("direction", out directionElement))
				direction = Vector3Utils.Vector3FromJSON(directionElement);

			return new Waypoint(name, radius, position, direction);
		}

		public static JSON.Element ToJSON(Waypoint point)
		{
			return JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
			{
				["name"] = JSON.Element.NewString(point.name),
				["radius"] = JSON.Element.NewNumber(point.radius),
				["position"] = Vector3Utils.Vector3ToJSON(point.position),
				["direction"] = Vector3Utils.Vector3ToJSON(point.direction),
			});
		}
	}

	struct Waypath
	{
		public readonly string name;
		public readonly IReadOnlyList<Waypoint> points;

		public Waypath(string name, List<Waypoint> points)
		{
			this.name = name;
			this.points = points;
			if (points == null)
				this.points = new List<Waypoint>();
		}

		public static JSON.Element ToJSON(Waypath path)
		{
			return JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
			{
				["name"] = JSON.Element.NewString(path.name),
				["points"] = JSON.Element.NewArray(path.points.Select(point => Waypoint.ToJSON(point)).ToList())
			});
		}

		public static Waypath FromJSON(JSON.Element rootElement)
		{
			var name = rootElement.jsonObject["name"].jsonString;
			var points = rootElement.jsonObject["points"].jsonArray
				.Select(element => Waypoint.FromJSON(element))
				.ToList();

			return new Waypath(name, points);
		}
	}
}
