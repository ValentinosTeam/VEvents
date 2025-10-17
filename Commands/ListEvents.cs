using System;
using System.Linq;
using System.Text;
using CommandSystem;
using GameCore;
using LabApi.Events;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using VEvents.Core;
using VEvents.Core.Interfaces;

namespace VEvents.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class ListEvents : ICommand
{
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		VEventManager eventManager = VEvents.Instance.EventManager;
		var sb = new StringBuilder();
		sb.AppendLine();
		foreach (IEvent ev in eventManager.Events) sb.AppendLine($"- {ev.Name}: {ev.Description}");

		Door door = Door.Get("049_ARMORY");
		Room room = door?.Rooms.First();
		if (room != null)
		{
			foreach (Door d in room.Doors)
			{
				if ((d.GetType() == typeof(Gate) && Math.Abs(d.Position.y - door.Position.y) <= 1) || d == door)
				{
					d.IsOpened = !d.IsOpened;
					Logger.Debug($"Name: {d.DoorName}, IsLocked: {d.IsLocked}, IsOpened: {d.IsOpened}, IsBreakable: {d is BreakableDoor}, Type: {d.GetType().Name}");
				}
			}
		}
		else Logger.Error("Room is null");

		response = sb.ToString();
		return true;
	}

	public string Command { get; } = "vlist";
	public string[] Aliases { get; } = [];
	public string Description { get; } = "Lists all available events.";
}