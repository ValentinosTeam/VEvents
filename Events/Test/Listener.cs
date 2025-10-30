using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using UnityEngine;
using VEvents.Extensions;
using Logger = LabApi.Features.Console.Logger;

namespace VEvents.Events.Test;

internal class Listener() : CustomEventsHandler
{
	public override void OnPlayerJumped(PlayerJumpedEventArgs ev)
	{
		Logger.Debug("Player jumped!");
	}
}