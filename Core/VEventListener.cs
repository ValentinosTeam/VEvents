using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;

namespace VEvents.Core;

public class VEventListener : CustomEventsHandler
{
	private VEventManager EventManager => VEvents.Instance.EventManager;

	public override void OnServerRoundEnded(RoundEndedEventArgs ev)
	{
		EventManager.StopAllEvents();
	}
}