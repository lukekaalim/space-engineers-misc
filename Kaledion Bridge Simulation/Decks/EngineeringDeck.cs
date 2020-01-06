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
		class EngineeringDeck
		{
			static string displayName = "Engineering Deck";
			static string shortName = "ENG";

			IMyTextPanel eventLogPanel;

			IMyTextPanel coolantPanel;
			IMyTextPanel powerPanel;

			IMyTextPanel statusPanel;

			static bool IsPanelMatchingDisplay(EntityAndMeta<IMyTextPanel> panel, string expectedDisplayType)
			{
				string displayType;
				var displayResult = panel.meta.TryGetValue("display", out displayType);
				if (!displayResult)
					return false;
				return displayType == expectedDisplayType;
			}

			public EngineeringDeck(List<EntityAndMeta<IMyTextPanel>> engineeringPanels)
			{
				eventLogPanel = engineeringPanels.Find(panel => IsPanelMatchingDisplay(panel, "events")).entity;
				coolantPanel = engineeringPanels.Find(panel => IsPanelMatchingDisplay(panel, "coolant")).entity;
				powerPanel = engineeringPanels.Find(panel => IsPanelMatchingDisplay(panel, "power")).entity;
				statusPanel = engineeringPanels.Find(panel => IsPanelMatchingDisplay(panel, "status")).entity;

				Initialize();
			}

			public void Initialize()
			{
				var panels = new[] { eventLogPanel, coolantPanel, powerPanel, statusPanel };
				foreach (var panel in panels)
				{
					panel.ContentType = ContentType.TEXT_AND_IMAGE;
					panel.Font = "Monotype";
					panel.FontSize = 0.7f;
					panel.TextPadding = 7.5f;
				}
				eventLogPanel.WriteText("Event Log Initialized");
				coolantPanel.WriteText("Coolant Initialized");
				powerPanel.WriteText("Power Initialized");
				statusPanel.WriteText("Status Initialized");
			}
		}
	}
}
