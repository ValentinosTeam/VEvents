using System;
using CommandSystem;
using LabApi.Features.Console;

namespace VEvents.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class StartEvent : ICommand
{
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (arguments.Count != 1)
		{
			response = "Usage: vstart <event_name>";
			return false;
		}
		string eventName = arguments.At(0);
		bool success = VEvents.Instance.EventManager.StartEvent(eventName, true);
		response = success ? $"Event '{eventName}' started successfully." : $"Event '{eventName}' not found or could not be started.";
		return success;
	}

	public string Command { get; } = "vstart";
	public string[] Aliases { get; } = [];
	public string Description { get; } = "Starts a specified event. Usage: vstart <event_name>";
}