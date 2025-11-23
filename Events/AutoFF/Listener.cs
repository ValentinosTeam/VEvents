using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using VEvents.Helpers;

namespace VEvents.Events.AutoFf;

public class Listener(Event ev) : CustomEventsHandler
{
	private readonly Event _event = ev;

	public override void OnServerRoundEnding(RoundEndingEventArgs ev)
	{
		Logger.Debug("Round Ending... Attempting to turn on friendly fire");
		if (!_event.CanRunThisRound()) return;
		RoundUtils.TurnFFOn();
		Logger.Debug("AutoFF turned on friendly fire");
	}

	public override void OnServerRoundStarted()
	{
		Logger.Debug("Round Ending... Attempting to turn off friendly fire");
		if (!_event.CanRunThisRound()) return;
		RoundUtils.TurnFFOff();
		Logger.Debug("AutoFF turned off friendly fire");
	}
}