using System.Collections.Generic;
using System.Linq;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using VEvents.Core;

namespace VEvents.Events.AutoFf;

public class Event : EventBase<Config>
{
	public override string Name { get; } = "AutoFF";

	private Listener Listener { get; set; }

	protected override void OnStart()
	{
		Logger.Debug("Started AutoFF!");
		Listener = new Listener(this);
		CustomHandlersManager.RegisterEventsHandler(Listener);
	}

	protected override void OnStop()
	{
		CustomHandlersManager.UnregisterEventsHandler(Listener);
		Listener = null;
	}

	public bool CanRunThisRound()
	{
		List<string> runningEventIds = VEvents.Instance.EventManager.GetRunningEventIds();
		foreach (string eventName in Config.CantRunWith) if (runningEventIds.Contains(eventName)) return false;

		return true;
	}
}