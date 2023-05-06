using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public class FactoryAPI
        {
            public const string BROADCAST_TAG = "lk.factory.api@1.0.0";

            public MyGridProgram program;
            public IMyBroadcastListener listener;
            IWoodsman woodsman;

            public class AssemblyRequest {
                public static explicit operator AssemblyRequest(Rial.Element element)
                {
                    return new AssemblyRequest();
                }
            }

            public void AcceptRequest(AssemblyRequest request)
            {
                List<IMyAssembler> assemblers = new List<IMyAssembler>();
                program.GridTerminalSystem.GetBlocksOfType(assemblers);


                //assemblers.Select(assembler => assembler.AddQueueItem())
            }

            public FactoryAPI(MyGridProgram program, IWoodsman woodsman)
            {
                this.woodsman = woodsman;
                this.program = program;
                listener = program.IGC.RegisterBroadcastListener(BROADCAST_TAG);

            }

            public void Update()
            {
                while (listener.HasPendingMessage)
                {
                    var message = listener.AcceptMessage();
                    var data = Rial.Binary.Read((ImmutableArray<byte>)message.Data);

                    var messageType = data.Object.Get("type").String;
                    switch (messageType)
                    {
                        case "ping":
                            {
                                var greeting = data.Object.Get("greeting").String;
                                woodsman.Log($"Recieved Ping \"{greeting}\", sending Pong");
                                program.IGC.SendBroadcastMessage(BROADCAST_TAG, Rial.Binary.Write(Rial.Object()
                                    .Prop("type", "pong")
                                    .Prop("greeting", "Hi!"))
                                    .ToImmutableArray());
                                break;
                            }
                        case "pong":
                            {
                                var greeting = data.Object.Get("greeting").String;
                                woodsman.Log($"Recieved Pong \"{greeting}\"");
                                break;
                            }
                    }
                }
            }

            public void SendPing()
            {
                var message = Rial.Binary.Write(Rial.Object()
                    .Prop("type", "ping")
                    .Prop("greeting", "Hello!")).ToImmutableArray();

                program.IGC.SendBroadcastMessage(BROADCAST_TAG, message);
                woodsman.Log("SENT PING");
            }
        }
    }
}
