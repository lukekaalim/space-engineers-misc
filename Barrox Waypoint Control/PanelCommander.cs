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
	interface ICommandable
	{
		PanelCommander.CommandTree GetCommandTree { get; }
	}

	class PanelCommander
	{
		public struct Command
		{
			public string label;
			public Action action;
		}

		public struct CommandTree
		{
			public string label;
			public List<Command> commands;
			public List<CommandTree> subtrees;
		}

		CommandTree root;
		List<int> subtreeIndexAtDepth;
		IMyTextSurface outputSurface;

		public PanelCommander(CommandTree root, IMyTextSurface outputSurface)
		{
			this.root = root;
			this.subtreeIndexAtDepth = new List<int>() { 0 };
			this.outputSurface = outputSurface;

			Draw();
		}

		List<string> BuildInterface(CommandTree tree, int subtreeDepth = 0)
		{
			List<string> lines;
			int subtreeIndex = subtreeIndexAtDepth[subtreeDepth];
			int maxDepth = subtreeIndexAtDepth.Count - 1;

			// If there's a index greater than where we are now,
			// don't try to render this command tree, but instead render the next one.
			if (maxDepth > subtreeDepth)
			{
				lines = BuildInterface(tree.subtrees[subtreeIndex - tree.commands.Count], subtreeDepth + 1);
				if (lines.Count < 9)
				{
					lines.Insert(0, "|| " + tree.label + $" [{subtreeIndex}]");
				}
				return lines;
			}
			lines = new List<string>() { "|| " + tree.label + $" [{subtreeIndex}]" };
			for (int i = 0; i < tree.commands.Count; i++)
			{
				if (subtreeIndex - i > 5)
				{
					continue;
				}
				if (i == subtreeIndex)
				{
					lines.Add("|> " + tree.commands[i].label);
				} else
				{
					lines.Add("|  " + tree.commands[i].label);
				}
			}
			for (int i = 0; i < tree.subtrees.Count; i++)
			{
				var commandIndex = subtreeIndex - tree.commands.Count;
				if (commandIndex - i > 5)
				{
					continue;
				}
				if (i == commandIndex)
				{
					lines.Add("|> " + tree.subtrees[i].label);
				}
				else
				{
					lines.Add("|  " + tree.subtrees[i].label);
				}
			}
			return lines;
		}

		CommandTree GetCurrentSubtree(CommandTree tree, int depth = 0)
		{
			int subtreeIndex = subtreeIndexAtDepth[depth];
			int maxDepth = subtreeIndexAtDepth.Count - 1;

			if (maxDepth > depth)
			{
				return tree.subtrees[subtreeIndex - tree.commands.Count];
			}
			return tree;
		}

		void Draw()
		{
			if (outputSurface == null)
				return;
			var lines = BuildInterface(root).Take(9).ToArray();
			var paddingLineCount = 9 - lines.Length;
			var paddedLines = paddingLineCount == 0 ? "" : new string('\n', paddingLineCount);
			var controlLine = "\n[1: Up]  [2: Down]  [3: Back] [4: Select]";
			outputSurface.WriteText(string.Join("\n", lines) + paddedLines + controlLine);
		}

		void Select(CommandTree commandTree, int index)
		{
			// it's a command
			if (index < commandTree.commands.Count)
			{
				commandTree.commands[index].action();
			} else
			// it's a subtree
			{
				subtreeIndexAtDepth.Add(0);
			}
		}

		void Back()
		{
			int maxDepth = subtreeIndexAtDepth.Count - 1;
			if (maxDepth > 0)
			{
				subtreeIndexAtDepth.RemoveAt(maxDepth);
			}
		}

		public void HandleCommands(string[] arguments)
		{
			var commandTree = GetCurrentSubtree(root);
			int maxDepth = subtreeIndexAtDepth.Count - 1;

			switch (arguments[0])
			{
				case "up":
					subtreeIndexAtDepth[maxDepth] = MathUtils.Mod(subtreeIndexAtDepth[maxDepth] + 1, commandTree.commands.Count + commandTree.subtrees.Count); break;
				case "down":
					subtreeIndexAtDepth[maxDepth] = MathUtils.Mod(subtreeIndexAtDepth[maxDepth] - 1, commandTree.commands.Count + commandTree.subtrees.Count); break;
				case "select":
					Select(commandTree, subtreeIndexAtDepth[maxDepth]); break;
				case "back":
					Back(); break;
				default:
					throw new Exception("Unrecognised Command");
			}
			Draw();
		}
	}
}
