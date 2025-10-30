using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Console;
using VEvents.Core.Interfaces;

namespace VEvents.Core;

/// <summary>
/// This is what starts and stops all the custom events. Commands use this to start events automatically and in the future this class will also start automatic events
/// </summary>
public class VEventManager
{
	public List<IEvent> Events { get; private set; } = [];
	public VEventManager()
	{
		AddEvent(new Events.ZombieSurvival.Event());
		AddEvent(new Events.Test.Event());

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
		if (manual && !ev.CanStartManually(out response))
		{
			response = "Event cannot be started manually.";
			return false;
		}
		if (!manual && !ev.CanStartAutomatically(out response))
		{
			response = "Event cannot be started automatically.";
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
		Logger.Debug("Added event: " + ev.Name);
		Events.Add(ev);
	}
}