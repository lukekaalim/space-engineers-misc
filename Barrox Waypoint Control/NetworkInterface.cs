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
	class NetworkInterface
	{
		readonly IMyIntergridCommunicationSystem IGC;
		readonly bool respondToPing;

		public NetworkInterface(IMyIntergridCommunicationSystem IGC, bool respondToPing = true)
		{
			this.IGC = IGC;
			this.respondToPing = respondToPing;
		}

		public void SendPing()
		{
			IGC.SendBroadcastMessage("ping", false);
		}

		void SendPong(long address)
		{
			IGC.SendUnicastMessage(address, "ping", true);
		}

		void HandlePing(MyIGCMessage message)
		{
			if (message.Source == IGC.Me)
				return;
			if (message.As<bool>() == true)
				return; // Recieved Pong
			if (respondToPing)
				SendPong(message.Source);
		}

		public void Tick()
		{
			while (IGC.UnicastListener.HasPendingMessage)
			{
				var message = IGC.UnicastListener.AcceptMessage();
				switch (message.Tag)
				{
					case "ping":
						HandlePing(message); break;
				}
			}
		}
	}
}
