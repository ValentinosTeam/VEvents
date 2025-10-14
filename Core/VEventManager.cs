using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Console;
using VEvents.Configs;
using VEvents.Core.Interfaces;
using VEvents.Events;

namespace VEvents.Core;

public class VEventManager
{
	public List<IEvent> Events { get; private set; } = [];
	public VEventManager()
	{
		AddEvent(new TestEvent());
		AddEvent(new ZombieSurvivalEvent());

		LoadEventConfigs();
	}

	public bool StartEvent(string name, bool manual)
	{
		IEvent ev = Events.Find(e => e.Name == name);
		if (ev == null) return false;
		if (manual && !ev.CanStartManually()) return false;
		if (!manual && !ev.CanStartAutomatically()) return false;
		ev.Start();
		return true;
	}

	public List<string> GetRunningEventNames()
	{
		return Events
			.Where(e => e.IsRunning)
			.Select(e => e.Name)
			.ToList();
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