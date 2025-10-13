using System;
using CommandSystem;

namespace VEvents.Commands;

public class ListEvents : ICommand
{
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		response = "";
		return false;
	}

	public string Command { get; } = "vlist";
	public string[] Aliases { get; } = [];
	public string Description { get; } = "Lists all available events.";
}