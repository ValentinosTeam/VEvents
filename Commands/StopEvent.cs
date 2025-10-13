using System;
using CommandSystem;

namespace VEvents.Commands;

public class StopEvent : ICommand
{
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		response = "";
		return false;
	}

	public string Command { get; } = "vstop";
	public string[] Aliases { get; } = [];
	public string Description { get; } = "Stops a specified event. Usage: vstop <event_name>";
}