using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Console;
using VEvents.Core.Interfaces;
using VEvents.Events;
using VEvents.Events.ZombieSurvival;

namespace VEvents.Core;

public class VEventManager
{
	public List<IEvent> Events { get; private set; } = [];
	public VEventManager()
	{
		AddEvent(new TestEvent());
		AddEvent(new Event());

		LoadEventConfigs();
	}

	public bool StartEvent(string name, out string response, bool manual = false)
	{
		response = null;
		IEvent ev = Events.Find(e => e.Name == name);
		if (ev == null)
		{
			response = "Event not found.";
			return false;
		}
		if (manual && !ev.CanStartManually())
		{
			response = "This event cannot be started manually.";
			return false;
		}
		if (!manual && !ev.CanStartAutomatically())
		{
			response = "This event cannot be started automatically.";
			return false;
		}
		if (ev.IsRunning)
		{
			response = "Event is already running.";
			return false;
		}
		ev.Start();
		return true;
	}

	public void StopAllEvents()
	{
		Logger.Debug("Stopping all events...");
		foreach (IEvent ev in Events.Where(ev => ev.IsRunning)) ev.Stop();
	}

	public bool StopEvent(string name, out string response)
	{
		IEvent ev = Events.Find(e => e.Name == name);
		if (ev == null)
		{
			response = "Event not found.";
			return false;
		}
		if (!ev.IsRunning)
		{
			response = "Event is not running.";
			return false;
		}

		ev.Stop();
		response = null;
		return true;
	}

	private void LoadEventConfigs()
	{
		foreach (IEvent ev in Events)
		{
			ev.LoadConfig();
		}
	}
	private void AddEvent(IEvent ev)
	{
		try
		{
			ev.Validate();
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to validate event {ev.Name}: {ex}");
			return;
		}
		Events.Add(ev);
	}
}