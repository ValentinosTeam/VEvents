using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;

namespace VEvents.Core;

/// <summary>
/// General Event Listener. Currently used to ensure that all events are stopped at round end.
/// </summary>
public class VEventListener : CustomEventsHandler
{
	private VEventManager EventManager => VEvents.Instance.EventManager;

	public override void OnServerRoundEnded(RoundEndedEventArgs ev)
	{
		EventManager.StopAllEvents();
	}
}