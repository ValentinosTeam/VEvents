using System.Collections.Generic;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using VEvents.Configs;
using VEvents.Core;
using VEvents.Core.Interfaces;

namespace VEvents.Events;

public class ZombieSurvivalConfig : EventConfig
{
	public float HiderSeekerRatio { get; set; } = 0.1667f; // 1/6th of players are seekers
}
public class ZombieSurvivalEvent : EventBase<ZombieSurvivalConfig>
{
	public override string Name { get; } = "ZombieSurvival";
	public override string Description { get; } = "Event turns off the lights and makes 2 teams, hiders and seekers. The seekers are zombies that spread the infection. Last hider standing at the end of the event wins.";
	protected override void OnStart()
	{
		Logger.Debug("Starting event...");
		Timing.RunCoroutine(WaitForRoundStart());
		// Make sure no other event is running
		// Wait until the round is started
		// Lock round from ending
		// Start a timer for the event duration
		// Repair, close and lock all doors
		// Create two teams: hiders and seekers
		// Randomly select a few players to be seekers (zombies) 1/6th of the players
		// Spawn hiders as d-boys in D-class quarters
		// Give every hider a lantern a gun and some ammo.
		// Spawn seekers as zombies in SCP049-chamber
		// Show help message to seekers and hiders.
		// In 15 seconds, unlock all doors but the SCP-049 chamber
		// Start a timer for 1 minute, show the time on screen.
		// Allow the SCP-049 elevator control to release the zombies. Turn off the lights. Announce that zombies are out
		// Start a timer for 15 minutes. Show the time on screen.
			// EVENT: If a hider dies by hand of another seeker, turn them into a zombie with slowness for 10 seconds and blindness on the spot.
			// EVENT: If a hider dies by any other means, they turn into a zombie and get teleported to SCP-049 chamber.
			// EVENT: If a seeker dies, they drop random loot and get teleported to SCP-049 chamber.
			// EVENT: If someone joins late, spawn them as seeker in SCP-049 chamber.
			// EVENT: If all hiders die, skip the timer.
		// When the timer ends, check win conditions:
			// If there are hiders alive, hiders win. Announce it.
			// If there are no hiders alive, seekers win. Announce it.
		// Teleport the winners to the tower for a party.
		// Restart the round after 30 seconds and end the event.
	}
	protected override void OnStop()
	{
	}

	public override bool CanStartManually()
	{
		Logger.Debug("Checking if event can be started manually...");
		//TODO: Make sure no other event is running.
		if (Round.IsRoundEnded) return false;

		return true;
	}

	private IEnumerator<float> WaitForRoundStart()
	{
		// Wait until the round is started
		yield return Timing.WaitUntilTrue(() => Round.IsRoundStarted);
		Logger.Debug("Round started, starting event...");
		PrepareRound();
	}

	private void PrepareRound()
	{
		// Repair, close and lock all doors
		Logger.Debug("Preparing round: repairing, closing and locking all doors...");
		foreach (Door door in Door.List)
		{
			if (door is BreakableDoor breakableDoor) breakableDoor.TryRepair();
			door.IsOpened = false;
			door.IsLocked = true; // NOTE: Visual bug if a broken door got fixed and locked, the button doesn't show the locked state. Still works though.
		}
		Logger.Debug("All doors repaired, closed and locked.");
	}

}