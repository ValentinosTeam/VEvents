using System;
using CommandSystem;

namespace VEvents.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class StopEvent : ICommand
{
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (arguments.Count != 1)
		{
			response = "Usage: vstop <event_name>";
			return false;
		}
		string eventName = arguments.At(0);
		bool success = VEvents.Instance.EventManager.StopEvent(eventName, out response);
		response = response == null ? $"Event {eventName} stopped successfully." : $"Event {eventName} could not be stopped: {response}";
		return success;
	}

	public string Command { get; } = "vstop";
	public string[] Aliases { get; } = [];
	public string Description { get; } = "Stops a specified event. Usage: vstop <event_name>";
}