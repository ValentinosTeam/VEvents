using System.Collections.Generic;
using VEvents.Core.Interfaces;
using VEvents.Events;

namespace VEvents.Core;

public class VEventManager
{
	private List<EventBase> _events = [];
	public VEventManager()
	{
		AddEvent(new TestEvent());
	}

	public void StartEvent(string name)
	{
		EventBase ev = _events.Find(e => e.Name == name);
		ev?.Start();
	}

	private void AddEvent(EventBase ev)
	{
		// TODO: Check if event is turned off in config
		_events.Add(ev);
	}
}