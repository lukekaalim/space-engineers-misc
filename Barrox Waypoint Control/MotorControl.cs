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
	class MotorControl
	{
		public struct MotorCommand : IEquatable<MotorCommand>
		{
			// a range from 0 to 1, where 0 is parked and 1 is max speed
			public readonly float forwardSpeed;
			// a range from -1 to 1, where -1 is left and 1 is right
			public readonly float turningAngle;
			public MotorCommand(float forwardSpeed, float turningAngle)
			{
				this.forwardSpeed = forwardSpeed;
				this.turningAngle = turningAngle;
			}
			public bool Equals(MotorCommand other)
			{
				return forwardSpeed == other.forwardSpeed &&
					turningAngle == other.turningAngle;
			}

			public override string ToString()
			{
				return $"Angle: {turningAngle}, Speed: {forwardSpeed}";
			}

			public static MotorCommand Stopped = new MotorCommand(0, 0);
		}

		readonly PositionalWheels wheels;
		readonly IMyRemoteControl mainControl;
		bool engaged;
		public MotorCommand LastCommand { get; private set; }

		public MotorControl(
			PositionalWheels wheels,
			IMyRemoteControl mainControl
		)
		{
			this.wheels = wheels;
			this.mainControl = mainControl;
		}

		public void Tick()
		{
			if (!engaged)
				return;

			var goingTooFast = mainControl.GetShipSpeed() > mainControl.SpeedLimit;
			mainControl.HandBrake = goingTooFast || LastCommand.forwardSpeed == 0;
		}

		void SetForwardSpeed(float forwardSpeed)
		{
			if (forwardSpeed == 0f)
			{
				// Stop forward movement
				foreach (var wheel in wheels.allWheels)
					wheel.SetValueFloat("Propulsion override", 0f);
				mainControl.HandBrake = true;
				return;
			}
			
			mainControl.HandBrake = false;
			foreach (var wheel in wheels.leftWheels)
				wheel.SetValueFloat("Propulsion override", forwardSpeed);
			foreach (var wheel in wheels.rightWheels)
				wheel.SetValueFloat("Propulsion override", -forwardSpeed);
		}

		void SetWheelDirection(float turningAngle)
		{
			if (turningAngle == 0)
			{
				foreach (var wheel in wheels.allWheels)
					wheel.SetValueFloat("Steer override", 0f);
				return;
			}

			foreach (var wheel in wheels.frontWheels)
				wheel.SetValueFloat("Steer override", turningAngle);
			foreach (var wheel in wheels.backWheels)
				wheel.SetValueFloat("Steer override", -turningAngle);
		}

		public void SetMotorCommand(MotorCommand command)
		{
			engaged = true;
			SetForwardSpeed(command.forwardSpeed);
			SetWheelDirection(command.turningAngle);
			LastCommand = command;
		}

		public void Disengage()
		{
			SetMotorCommand(MotorCommand.Stopped);
			engaged = false;
		}
	}
}
