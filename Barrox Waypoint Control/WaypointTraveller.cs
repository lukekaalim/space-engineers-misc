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
	class WaypointTraveller
	{
		public readonly PanelCommander.CommandTree commandTree;
		readonly MotorControl motorControl;
		readonly IMyRemoteControl mainControl;
		readonly IMyTextSurface outputSurface;

		Waypath path;
		int currentPointIndex;

		public enum TravelState
		{
			Travelling,
			Stopped,
			AtDestination,
		}
		TravelState travelState;

		public WaypointTraveller(MotorControl motorControl, IMyRemoteControl mainControl, IMyTextSurface outputSurface)
		{
			this.motorControl = motorControl;
			this.mainControl = mainControl;
			this.outputSurface = outputSurface;

			this.currentPointIndex = 0;
			this.travelState = TravelState.Stopped;
			this.path = new Waypath("Default Path", new List<Waypoint>());

			commandTree = new PanelCommander.CommandTree()
			{
				label = "Waypath Traveller",
				commands = new List<PanelCommander.Command>() {
					new PanelCommander.Command() { label = "Travel", action = Travel },
					new PanelCommander.Command() { label = "Pause", action = Pause },
					new PanelCommander.Command() { label = "Skip", action = Skip },
					new PanelCommander.Command() { label = "Back", action = Back },
					new PanelCommander.Command() { label = "Reset", action = Reset },
					new PanelCommander.Command() { label = "Stop", action = Stop },
				},
				subtrees = new List<PanelCommander.CommandTree>(),
			};
		}

		public JSON.Element SaveState()
		{
			return JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
			{
				["travelState"] = JSON.Element.NewString(travelState.ToString()),
				["currentPointIndex"] = JSON.Element.NewNumber(currentPointIndex),
			});
		}

		public void LoadState(JSON.Element element)
		{
			travelState = (TravelState)Enum.Parse(typeof(TravelState), element.jsonObject["travelState"].jsonString);
			currentPointIndex = (int)element.jsonObject["currentPointIndex"].jsonNumber;
		}

		MotorControl.MotorCommand CalculateMotorCommand()
		{
			var controlPosition = mainControl.GetPosition();
			var mainControlOrientation = mainControl.WorldMatrix.GetOrientation();
			var directionToPosition = Vector3.Normalize(target - controlPosition);

			var distanceFromLeft = Vector3.Distance(mainControlOrientation.Left, directionToPosition);
			var distanceFromRight = Vector3.Distance(mainControlOrientation.Right, directionToPosition);
			var distanceFromFront = Vector3.Distance(mainControlOrientation.Forward, directionToPosition);

			// This number ranges from -1 (left) to 1 (right)
			float turningPower = (distanceFromLeft - distanceFromRight) / 2;
			float speed = 1;

			if (distanceFromFront > 1)
				// Turn hardest if the point is behind us
				turningPower = turningPower / Math.Abs(turningPower);

			if (Math.Abs(turningPower) > 0.5)
				// Slow down if we're turning hard
				speed = 0.3f;

			return new MotorControl.MotorCommand(
				speed,
				turningPower
			);
		}

		public void SetPath(Waypath path)
		{
			this.path = path;
			this.currentPointIndex = 0;
		}

		public void Pause()
		{
			travelState = TravelState.Stopped;
			motorControl.SetMotorCommand(MotorControl.MotorCommand.Stopped);
		}

		public void Travel()
		{
			travelState = TravelState.Travelling;
		}

		public void Stop()
		{
			travelState = TravelState.Stopped;
			motorControl.Disengage();
		}

		void Skip()
		{
			if (currentPointIndex < path.points.Count - 1)
				currentPointIndex++;
		}

		void Back()
		{
			if (currentPointIndex > 0)
				currentPointIndex++;
		}
		void Reset()
		{
			currentPointIndex = 0;
		}

		public void DrawOutput()
		{
			if (outputSurface == null)
				return;
			if (path.points.Count == 0)
			{
				outputSurface.WriteText(string.Join("\n",
					"Travel State: " + travelState.ToString(),
					"Motor State: " + motorControl.LastCommand.ToString(),
					"Target Name: " + "N\\A",
					"Distance: " + "N\\A"
				));
			}

			if (currentPointIndex >= path.points.Count)
				return;
			var currentPoint = path.points[currentPointIndex];

			outputSurface.WriteText(string.Join("\n",
				"Travel State: " + travelState.ToString(),
				"Motor State: " + motorControl.LastCommand.ToString(),
				"Target Motor State: " + CalculateMotorCommand().ToString(),
				"Target Name: " + currentPoint.name,
				"Distance To Interpolant: " + Vector3.Distance(mainControl.GetPosition(), target),
				"Distance: " + Vector3.Distance(mainControl.GetPosition(), currentPoint.position),
				(currentPointIndex + 1) + "/" + path.points.Count
			));
		}

		Vector3 target;
		bool calculatedTarget;

		public Vector3[] GetTargetSpline()
		{
			var travellerPosition = mainControl.GetPosition();
			var travellerForwardDirection = mainControl.WorldMatrix.GetOrientation().Forward;

			var controlStrength = 10;
			var pointStrength = 10;

			var point = path.points[currentPointIndex];
			var points = new Vector3[] {
				travellerPosition,
				travellerPosition + (travellerForwardDirection * controlStrength),
				point.position + (Vector3.Negate(point.direction) * pointStrength),
				point.position,
			};

			return points;
		}

		public void Tick100()
		{
		}

		public bool AtCurrentPoint()
		{
			var currentPoint = path.points[currentPointIndex];
			var distanceToCurrentPoint = Vector3.Distance(mainControl.GetPosition(), currentPoint.position);
			var distanceToCurrentPointDirection = Vector3.Distance(mainControl.WorldMatrix.GetOrientation().Forward, currentPoint.direction);

			return (
				distanceToCurrentPoint < currentPoint.radius &&
				distanceToCurrentPointDirection < currentPoint.angularDeviation
			);
		}

		public void Tick()
		{
			DrawOutput();
			if (path.points.Count == 0)
				return;
			if (travelState != TravelState.Travelling)
				return;
			var spline = GetTargetSpline();

			var targetDistance = 20;
			target = SplineUtils.GetAbsolutePointOnSpline(spline, SplineUtils.GetSplineLength(spline, 5), targetDistance);

			if (AtCurrentPoint())
			{
				// If this is the last point
				if (path.points.Count - 1 == currentPointIndex)
				{
					// Stop moving
					travelState = TravelState.AtDestination;
					motorControl.SetMotorCommand(MotorControl.MotorCommand.Stopped);
					return;
				}
				else
				{
					// Or update the point to the next one
					currentPointIndex++;
				}
			}

			MotorControl.MotorCommand command = CalculateMotorCommand();
			if (!command.Equals(motorControl.LastCommand))
			{
				motorControl.SetMotorCommand(command);
			}
		}
	}
}
