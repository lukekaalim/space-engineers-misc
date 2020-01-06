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
	class WaypathRecorder
	{
		public readonly PanelCommander.CommandTree commandTree;
		readonly IMyRemoteControl mainControl;
		readonly IMyTextSurface outputSurface;
		readonly Action<Waypath> onPathUpdate;

		readonly List<Waypoint> pathPoints;
		string pathName;

		int greedlesDeployed;

		public WaypathRecorder(
			IMyRemoteControl mainControl,
			Waypath initalPath,
			IMyTextSurface outputSurface,
			Action<Waypath> onPathUpdate
		)
		{
			this.mainControl = mainControl;
			this.outputSurface = outputSurface;
			this.pathName = initalPath.name;
			this.pathPoints = initalPath.points.ToList();
			this.onPathUpdate = onPathUpdate;

			commandTree = new PanelCommander.CommandTree()
			{
				label = "Recorder",
				commands = new List<PanelCommander.Command>() {
					new PanelCommander.Command() { label = "Record", action = RecordPoint },
					new PanelCommander.Command() { label = "Undo", action = Undo },
					new PanelCommander.Command() { label = "Reset", action = Reset },
					new PanelCommander.Command() { label = "Reverse", action = Reverse }
				},
				subtrees = new List<PanelCommander.CommandTree>(),
			};

			DrawOutput();
		}

		void DrawOutput()
		{
			if (outputSurface == null)
				return;
			outputSurface.WriteText(string.Join("\n",
				"Waypath Recorder",
				"Recording: " + pathName,
				"Points: " + pathPoints.Count,
				"Position: " + mainControl.GetPosition(),
				"Direction: " + mainControl.WorldMatrix.GetOrientation().Forward
			));
		}

		public void Tick()
		{
			DrawOutput();
		}

		public Vector3 OffsetPositionFromPlanet(Vector3 position, float offset = 30)
		{
			Vector3D planetPosition;
			bool isNearPlanet = mainControl.TryGetPlanetPosition(out planetPosition);
			if (!isNearPlanet)
				return position;
			Vector3 directionToPlanet = Vector3.Normalize(planetPosition - position);
			Vector3 offsetPosition = position + Vector3.Negate(directionToPlanet) * offset;
			return offsetPosition;
		}

		void Reverse()
		{
			pathPoints.Reverse();
			onPathUpdate(GetPath());
		}

		public void RecordPoint()
		{
			pathPoints.Add(new Waypoint() {
				name = "Unnamed Point",
				position = mainControl.GetPosition(),
				radius = 10f,
				direction = mainControl.WorldMatrix.GetOrientation().Forward
			});
			onPathUpdate(GetPath());
		}
		public void Undo()
		{
			pathPoints.RemoveAt(pathPoints.Count - 1);
			onPathUpdate(GetPath());
		}

		public void Reset()
		{
			pathPoints.Clear();
			onPathUpdate(GetPath());
		}

		public void HandleArguments(string[] arguments)
		{
			switch (arguments[0])
			{
				case "record":
					RecordPoint(); break;
				case "reset":
					Reset(); break;
				case "undo":
					Undo(); break;
			}
		}

		public void SetPath(Waypath path)
		{
			pathName = path.name;
			pathPoints.Clear();
			pathPoints.AddRange(path.points);
		}

		public Waypath GetPath()
		{
			return new Waypath(pathName, pathPoints.ToList());
		}
	}
}
