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
using VRage.Scripting;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public interface IWoodsman
        {
            void Log(string message);
        }

        public class RootWoodsman : IWoodsman
        {
            IMyTextSurface panel;
            Queue<string> messages = new Queue<string>();
            Program program;


            public RootWoodsman(Program program)
            {
                this.program = program;
                for (int i = 0; i < program.Me.SurfaceCount; i++)
                {
                    program.Echo(program.Me.GetSurface(i).DisplayName);
                }

                panel = program.Me.GetSurface(0);
            }

            public void Log(string message)
            {
                messages.Enqueue(message);
                if (messages.Count > 10)
                    messages.Dequeue();
            }

            public void Draw()
            {
                panel.WriteText(string.Join("\n", messages));
            }

            public BranchWoodsman Branch(string tag)
            {
                return new BranchWoodsman() { Tag = tag, Parent = this, program = program };
            }
        }

        public class BranchWoodsman : IWoodsman
        {
            public string Tag;
            public IWoodsman Parent;
            public Program program;
            public void Log(string message)
            {
                Parent.Log($"[{Tag}] {message}");
            }
        }
    }
}
