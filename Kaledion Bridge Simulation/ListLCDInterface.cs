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
	partial class Program
	{
		public class ListLCDInterface
		{
			IMyTextPanel output;
			List<string> elements;
			string title;
			int currentIndex = 0;
			public ListLCDInterface(IMyTextPanel output, string title, List<string> elements)
			{
				this.output = output;
				this.title = title;
				this.elements = elements;

				output.ContentType = ContentType.TEXT_AND_IMAGE;
				output.Font = "Monospace";
				DrawInterface();
			}

			void DrawInterface()
			{
				string textToDraw = title + " [" + currentIndex.ToString() + "]";
				for (int i = 0; i < elements.Count; i++)
				{
					string selectBox = "[ ]";
					if (i == currentIndex)
					{
						selectBox = "[X]";
					}
					string indexBox = "[" + i.ToString() + "]";
					textToDraw += '\n' + " " + selectBox + " " + indexBox + " " + elements[i];
				}
				output.WriteText(textToDraw);
			}

			public void IncreaseIndex()
			{
				currentIndex = Mod(currentIndex + 1, elements.Count);
				DrawInterface();
			}
			public void DecreaseIndex()
			{
				currentIndex = Mod(currentIndex - 1, elements.Count);
				DrawInterface();
			}
		}
	}
}
