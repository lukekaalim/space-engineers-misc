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
	struct PositionalWheels
	{
		public readonly IEnumerable<IMyMotorSuspension> frontWheels;
		public readonly IEnumerable<IMyMotorSuspension> backWheels;
		public readonly IEnumerable<IMyMotorSuspension> leftWheels;
		public readonly IEnumerable<IMyMotorSuspension> rightWheels;
		public readonly IEnumerable<IMyMotorSuspension> allWheels;

		public PositionalWheels(
			IEnumerable<IMyMotorSuspension> frontWheels,
			IEnumerable<IMyMotorSuspension> backWheels,
			IEnumerable<IMyMotorSuspension> leftWheels,
			IEnumerable<IMyMotorSuspension> rightWheels,
			IEnumerable<IMyMotorSuspension> allWheels
		)
		{
			this.frontWheels = frontWheels;
			this.backWheels = backWheels;
			this.leftWheels = leftWheels;
			this.rightWheels = rightWheels;
			this.allWheels = allWheels;
		}
	}

	partial class Program : MyGridProgram
	{
		readonly List<Waypath> paths = new List<Waypath>() { new Waypath(name: "Default Waypath", points: new List<Waypoint>()) };
		int currentPathIndex = 0;

		Ticker ticker;
		MotorControl motor;
		GreedleManager greedleManager;
		WaypathRecorder recorder;

		IMyRemoteControl mainControl;
		IMyTextSurface pathOutput;

		WaypointTraveller traveller;
		PanelCommander commander;

		public Program()
		{
			Runtime.UpdateFrequency = UpdateFrequency.Update10 | UpdateFrequency.Update100;
			Initalize();
		}

		JSON.Element SerializeState ()
		{
			return JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
			{
				["currentPathIndex"] = JSON.Element.NewNumber(currentPathIndex),
				["paths"] = JSON.Element.NewArray(paths.Select(path => Waypath.ToJSON(path)).ToList()),
				["traveler"] = traveller.SaveState()
			});
		}

		void DeserializeState(JSON.Element state)
		{
			paths.Clear();
			paths.AddRange(
				state
				.jsonObject["paths"].jsonArray
				.Select(element => Waypath.FromJSON(element))
			);
			currentPathIndex = (int)state.jsonObject["paths"].jsonNumber;
			if (paths.Count < 1)
			{
				paths.Add(new Waypath(name: "Default Waypath", points: new List<Waypoint>()));
				currentPathIndex = 0;
			}
			recorder.SetPath(paths[currentPathIndex]);
			traveller.SetPath(paths[currentPathIndex]);
			traveller.LoadState(state.jsonObject["traveler"]);
			DrawPaths();
		}

		public void Save()
		{
			Storage = JSON.Stringify(SerializeState());
		}
		public void Load()
		{
			try
			{
				ticker.CreatePromise(
					JSON.Parse(Storage).GetEnumerator(),
					status => {
						if (status.state == JSON.ParserStatusState.Error)
							Me.GetSurface(0).WriteText("Loading from Storage Error:\n" + status.errorMessage + "\n", true);
						else
							DeserializeState(status.result.Value);
					},
					500
				);
			} catch {
				Echo("Failure to Load from Storage");
			}
		}

		void SaveToData ()
		{
			Me.CustomData = JSON.PrettyStringify(SerializeState());
		}

		void LoadFromData()
		{
			try
			{
				ticker.CreatePromise(
					JSON.Parse(Me.CustomData).GetEnumerator(),
					status => {
						Me.GetSurface(0).WriteText("Status Recieved", true);
						if (status.state == JSON.ParserStatusState.Error)
							Me.GetSurface(0).WriteText(
								$"Loading from Data Error@: {status.charactersProcessed}/{Me.CustomData.Length}\n"
								+ status.errorMessage
								+ $"\n", true);
						else
							DeserializeState(status.result.Value);
					},
					500
				);
			}
			catch
			{
				Echo("Failure to Load from Custom Data");
			}
		}

		static IMyMotorSuspension[] GetWheelsByPosition(IEnumerable<IMyMotorSuspension> wheels, List<JSON.Element> wheelMetadatas, string position)
		{
			return wheels
				.Where((wheel, index) =>
					// each wheel should have a  { "positions": ["front", "right"] } JSON metadata associated with it
					// we want the "positions" array
					wheelMetadatas[index].jsonObject["positions"].jsonArray
					.Select(element => element.jsonString)
					// and we want to check that is has the position we're looking for
					.Contains(position)
				)
				.ToArray();
		}

		static JSON.Element defaultWheelMeta = JSON.Element.NewObject(new Dictionary<string, JSON.Element>()
		{
			["positions"] = JSON.Element.NewArray(new List<JSON.Element>()),
		});

		PositionalWheels DetectWheels()
		{
			var wheels = new List<IMyMotorSuspension>();
			GridTerminalSystem.GetBlocksOfType(wheels);
			var wheelsMeta = wheels.Select(wheel =>
			{
				try
				{
					return JSON.SyncParse(wheel.CustomData);
				} catch
				{
					return defaultWheelMeta;
				}
			}).ToList();

			return new PositionalWheels(
				frontWheels: GetWheelsByPosition(wheels, wheelsMeta, "front"),
				backWheels: GetWheelsByPosition(wheels, wheelsMeta, "back"),
				leftWheels: GetWheelsByPosition(wheels, wheelsMeta, "left"),
				rightWheels: GetWheelsByPosition(wheels, wheelsMeta, "right"),
				allWheels: wheels
			);
		}

		List<IMyGyro> DetectGyroscopes()
		{
			var gyroscopes = new List<IMyGyro>();
			GridTerminalSystem.GetBlocksOfType(gyroscopes);
			return gyroscopes;
		}

		IMyRemoteControl DetectControls()
		{
			var controls = new List<IMyRemoteControl>();
			GridTerminalSystem.GetBlocksOfType(controls);
			return controls.Find(control => JSON.SyncParse(control.CustomData).jsonObject["control"].jsonString == "primary");
		}

		List<IMyCockpit> DetectCockpits()
		{
			var cockpits = new List<IMyCockpit>();
			GridTerminalSystem.GetBlocksOfType(cockpits);
			return cockpits.FindAll((cockpit) => {
				try
				{
					return JSON.SyncParse(cockpit.CustomData).jsonObject["shouldDisplayBarroxOutput"].jsonBoolean;
				} catch
				{
					return false;
				}
			});
		}

		IMyTextSurface[] DetectPanels(List<IMyCockpit> cockpits)
		{
			return cockpits.SelectMany(cockpit =>
			{
				var cockpitSurfaces = new IMyTextSurface[cockpit.SurfaceCount];
				for (int i = 0; i < cockpit.SurfaceCount; i++)
				{
					var surface = cockpit.GetSurface(i);
					surface.ContentType = ContentType.TEXT_AND_IMAGE;
					surface.WriteText(i.ToString());
					cockpitSurfaces[i] = surface;
				}
				return cockpitSurfaces;
			}).ToArray();
		}

		void OnRecorderPathUpdate(Waypath newPath)
		{
			paths[currentPathIndex] = newPath;
			traveller.SetPath(newPath);
			DrawPaths();
		}

		void Initalize()
		{
			var positionedWheels = DetectWheels();
			var control = DetectControls();
			var cockpits = DetectCockpits();
			var displayPanels = DetectPanels(cockpits);
			var gyros = DetectGyroscopes();

			this.ticker = new Ticker();
			this.motor = new MotorControl(positionedWheels, control);

			this.mainControl = control;
			this.traveller = new WaypointTraveller(motor, control, displayPanels[0]);
			this.greedleManager = new GreedleManager(IGC, null);
			this.recorder = new WaypathRecorder(
				mainControl,
				new Waypath("Default Waypath", new List<Waypoint>()),
				displayPanels[1],
				OnRecorderPathUpdate
			);
			this.pathOutput = displayPanels[2];

			this.commander = new PanelCommander(
				new PanelCommander.CommandTree() {
					label = "Main Menu",
					subtrees = new List<PanelCommander.CommandTree>()
					{
						traveller.commandTree,
						greedleManager.commandTree,
						recorder.commandTree,
					},
					commands = new List<PanelCommander.Command>()
					{
						new PanelCommander.Command() { label = "Save State to CustomData", action = SaveToData },
						new PanelCommander.Command() { label = "Load State from CustomData", action = LoadFromData },
						new PanelCommander.Command() { label = "Set Greedles to Path", action = DeployGreedles },
						new PanelCommander.Command() { label = "Switch Waypath", action = NextWaypath },
						new PanelCommander.Command() { label = "New Waypath", action = StartNewWaypath },
						new PanelCommander.Command() { label = "Delete Waypath", action = RemoveWaypath },
						new PanelCommander.Command() { label = "Dump Traveller GPS Data to Surface", action = TravellerDump },
					}
				},
				displayPanels[displayPanels.Length - 2]
			);

			Load();
			DrawPaths();
		}

		void TravellerDump()
		{
			var spline = traveller.GetTargetSpline();
			var targetDistance = 20;
			var target = SplineUtils.GetAbsolutePointOnSpline(spline, SplineUtils.GetSplineLength(spline, 5), 100);
			Me.GetSurface(0).WriteText(string.Join("\n",
				SplineUtils.SampleSplineToLine(spline, 10)
					.Select((position, index) => GPS.Stringify(new GPS.Position($"Spline-{index}", position)))
					.Concat(
						spline.Select((position, index) => GPS.Stringify(new GPS.Position($"Control Points-{index}", position)))
					)
					.Concat(new List<string>()
					{
						GPS.Stringify(new GPS.Position("Target", target)),
					})
					.Concat(
						paths[currentPathIndex].points.Select((point, index) => GPS.Stringify(new GPS.Position($"Waypoint Position-{index}", point.position)))
					)
					.Concat(
						paths[currentPathIndex].points.Select((point, index) => GPS.Stringify(new GPS.Position($"Waypoint Direction-{index}", point.position + (point.direction * 3) )))
					)
			));
		}

		void DeployGreedles()
		{
			greedleManager.SendGreedles(paths[currentPathIndex].points.Select(point => recorder.OffsetPositionFromPlanet(point.position)).ToArray());
		}

		void DrawPaths()
		{
			if (paths.Count == 0)
			{
				pathOutput.WriteText($"Paths [0/0]\nNo Paths");
				return;
			}
			pathOutput.WriteText(
				$"Paths [{currentPathIndex + 1}/{paths.Count}]\n" +
				string.Join("\n", paths.Select((path, index) =>
					$"[{(index == currentPathIndex ? "X" : " ")}][{index}] {path.name}"
				))
			);
		}

		void NextWaypath()
		{
			currentPathIndex = MathUtils.Mod(currentPathIndex + 1, paths.Count);
			traveller.SetPath(paths[currentPathIndex]);
			recorder.SetPath(paths[currentPathIndex]);
			DrawPaths();
		}

		void StartNewWaypath()
		{
			paths.Add(new Waypath(name: "New Waypath", points: new List<Waypoint>()));
			currentPathIndex = paths.Count - 1;
			traveller.SetPath(paths[currentPathIndex]);
			recorder.SetPath(paths[currentPathIndex]);
			DrawPaths();
		}

		void RemoveWaypath()
		{
			if (paths.Count == 1)
				return;

			paths.RemoveAt(currentPathIndex);
			currentPathIndex = 0;
			traveller.SetPath(paths[currentPathIndex]);
			recorder.SetPath(paths[currentPathIndex]);
			DrawPaths();
		}

		void Tick()
		{
			traveller.Tick();
			motor.Tick();
			recorder.Tick();
			ticker.Tick();

			Echo($"%{((float)Runtime.CurrentInstructionCount / (float)Runtime.MaxInstructionCount) * 100} of allocated instructions used");
		}

		void Tick100()
		{
			traveller.Tick100();
		}

		void IGCTick()
		{
			greedleManager.Tick();
		}

		public void Main(string argumentText, UpdateType updateSource)
		{
			Echo(new Random().NextDouble().ToString());
			Echo(updateSource.ToString());
			Echo(argumentText);

			if (updateSource.HasFlag(UpdateType.Update10))
				Tick();
			if (updateSource.HasFlag(UpdateType.Update100))
				Tick100();
			if (updateSource.HasFlag(UpdateType.IGC))
				IGCTick();

			var arguments = argumentText.Split(' ');
			if (arguments.Length < 1)
				return;
			switch (arguments[0])
			{
				case "init":
					Initalize(); break;
				case "save":
					Save(); break;
				case "command":
					commander.HandleCommands(arguments.Skip(1).ToArray()); break;
			}
		}
	}
}
