using System.Collections.Generic;
using VEvents.Configs;
using VEvents.Core.Interfaces;
using VEvents.Events;

namespace VEvents.Core;

public class VEventManager
{
	private List<IEvent> _events = [];
	public VEventManager()
	{
		AddEvent(new TestEvent());
		AddEvent(new ZombieSurvivalEvent());

		LoadEventConfigs();
	}

	public void StartEvent(string name)
	{
		IEvent ev = _events.Find(e => e.Name == name);
		ev?.Start();
	}

	public void LoadEventConfigs()
	{
		foreach (IEvent ev in _events)
		{
			ev.LoadConfig();
		}
	}

	private void AddEvent(IEvent ev)
	{
		// TODO: Check if event is turned off in config
		_events.Add(ev);
	}
}