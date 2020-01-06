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
		enum DroneState
		{
			Hibernating,
			Journying,
			Arrived
		}

		IMyRemoteControl mainControl;
		IMyBroadcastListener directiveListener;
		string name;
		Vector3 target;

		public Program()
		{
			Initalize();
		}

		void Target(Vector3 position)
		{
			target = position;
			mainControl.ClearWaypoints();
			mainControl.AddWaypoint(position, "Target Destination");
			mainControl.FlightMode = FlightMode.OneWay;
			mainControl.SetAutoPilotEnabled(true);
		}

		void HandleMessage()
		{
			while (directiveListener.HasPendingMessage)
			{
				var directive = GreedleDirective.FromJSON(JSON.Parse(directiveListener.AcceptMessage().As<string>()));
				switch (directive.type)
				{
					case GreedleDirective.DirectiveType.Respond:
						IGC.SendBroadcastMessage(
							"greedle_response",
							JSON.Stringify(GreedleResponse.ToJSON(new GreedleResponse() {
								name = name,
								position = mainControl.GetPosition()
							} ))
						); break;
					case GreedleDirective.DirectiveType.Target:
						{
							if (directive.targetDirective.addressedTo == name)
							{
								Target(directive.targetDirective.target);
							}
							break;
						}
				}
			}
		}

		void Initalize()
		{
			var controls = new List<IMyRemoteControl>();
			GridTerminalSystem.GetBlocksOfType(controls);

			mainControl = controls.Find(control => {
				try { return JSON.Parse(control.CustomData).jsonObject["type"].jsonString == "mainControl"; }
				catch { return false; }
			});
			name = $"Greedle ID {new Random().Next()}";
			target = Vector3.Zero;
			directiveListener = IGC.RegisterBroadcastListener("greedle_directive");
			directiveListener.SetMessageCallback();

			var antennas = new List<IMyRadioAntenna>();
			GridTerminalSystem.GetBlocksOfType(antennas);
			foreach (var antenna in antennas)
			{
				antenna.HudText = name;
			}
			Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
		}

		public void Main(string argument, UpdateType updateSource)
		{
			Me.GetSurface(0).WriteText(string.Join("\n",
				new Random().NextDouble().ToString(),
				updateSource.ToString(),
				argument
			));

			switch (updateSource)
			{
				case UpdateType.IGC:
					HandleMessage(); break;
				case UpdateType.Terminal:
				case UpdateType.Trigger:
					switch (argument)
					{
						case "init":
							Initalize(); break;
					}
					break;
			}
		}
	}
}
