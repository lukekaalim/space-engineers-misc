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
	struct GreedleDirective
	{
		public struct TargetDirective
		{
			public string addressedTo;
			public Vector3 target;
		}

		public enum DirectiveType
		{
			Respond,
			Target
		}

		public DirectiveType type;
		public TargetDirective targetDirective;

		public static GreedleDirective FromJSON(JSON.Element rootElement)
		{
			var type = rootElement.jsonObject["type"].jsonString;
			switch (type)
			{
				case "target":
					return new GreedleDirective()
					{
						type = DirectiveType.Target,
						targetDirective = new TargetDirective()
						{
							addressedTo = rootElement.jsonObject["addressedTo"].jsonString,
							target = Vector3Utils.Vector3FromJSON(rootElement.jsonObject["target"]),
						},
					};
				case "respond":
					return new GreedleDirective()
					{
						type = DirectiveType.Respond,
					};
			}
			throw new Exception("Unknown Directive Type");
		}

		public static JSON.Element ToJSON(GreedleDirective directive)
		{
			switch (directive.type)
			{
				case DirectiveType.Target:
					return JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
					{
						["type"] = JSON.Element.NewString("target"),
						["addressedTo"] = JSON.Element.NewString(directive.targetDirective.addressedTo),
						["target"] = Vector3Utils.Vector3ToJSON(directive.targetDirective.target),
					});
				case DirectiveType.Respond:
					return JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
					{
						["type"] = JSON.Element.NewString("respond"),
					});
			}
			throw new Exception("Unknown Directive Type");
		}
	}

	struct GreedleResponse
	{
		public string name;
		public Vector3 position;

		public static GreedleResponse FromJSON(JSON.Element rootElement)
		{
			return new GreedleResponse() {
				name = rootElement.jsonObject["name"].jsonString,
				position = Vector3Utils.Vector3FromJSON(rootElement.jsonObject["position"])
			};
		}

		public static JSON.Element ToJSON(GreedleResponse response)
		{
			return JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
			{
				["name"] = JSON.Element.NewString(response.name),
				["position"] = Vector3Utils.Vector3ToJSON(response.position),
			});
		}
	}
}
