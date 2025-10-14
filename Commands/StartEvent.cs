using System;
using CommandSystem;
using LabApi.Features.Console;

namespace VEvents.Commands;

public class StartEvent : ICommand
{
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		Logger.Info(arguments);
		response = "";
		return false;
	}

	public string Command { get; } = "vstart";
	public string[] Aliases { get; } = [];
	public string Description { get; } = "Starts a specified event. Usage: vstart <event_name>";
}