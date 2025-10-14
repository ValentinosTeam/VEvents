using System.Collections.Generic;
using System.Linq;
using CommandSystem.Commands.RemoteAdmin.Dummies;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Spawnpoints;
using UnityEngine;
using VEvents.Configs;
using VEvents.Core;
using VEvents.Core.Interfaces;
using VEvents.Helpers;
using Logger = LabApi.Features.Console.Logger;

namespace VEvents.Events.ZombieSurvival;

// Make sure no other event is running.
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
public class ZombieSurvivalEvent : EventBase<ZombieSurvivalConfig>
{
	public override string Name { get; } = "ZombieSurvival";
	public override string Description { get; } = "Event turns off the lights and makes 2 teams, hiders and seekers. The seekers are zombies that spread the infection. Last hider standing at the end of the event wins.";

	private List<Player> _hiders;
	private List<Player> _seekers;

	protected override void OnStart()
	{
		Logger.Debug("Starting event...");
		Timing.RunCoroutine(EventStartup());
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

	private IEnumerator<float> EventStartup()
	{
		// Wait until the round is started
		yield return Timing.WaitUntilTrue(() => Round.IsRoundStarted);
		Logger.Debug("Round started, starting event...");
		RoundUtils.LockRound();
		PlayerUtils.PlayersToSpectators();
		MapUtils.CloseAndLockAllDoors();
		PlayerUtils.SplitIntoTwoTeams(out _seekers, out _hiders, Settings.SeekerRatio);
		SpawnPlayers();
		CooldownUtils.Start(
			duration: Settings.GuideMessageInterval * Settings.SeekerGuideMessages.Count,
			interval: Settings.GuideMessageInterval,
			onInterval: (remaining, iteration) =>
			{
				foreach (Player seeker in _seekers)
				{
					Server.SendBroadcast(seeker, Settings.SeekerGuideMessages[iteration], 11, Broadcast.BroadcastFlags.Normal, true);
				}
				foreach (Player hiders in _hiders)
				{
					Server.SendBroadcast(hiders, Settings.HiderGuideMessages[iteration], 11, Broadcast.BroadcastFlags.Normal, true);
				}
			},
			onFinish: ReleaseZombies
		);
	}

	private void ReleaseZombies()
	{
		MapUtils.UnlockAllDoors();
		//TODO: Keep the zombies locked in SCP-049 chamber

		CooldownUtils.Start(
			duration: Settings.ZombieReleaseDelay,
			interval: 1f,
			onInterval: (remaining, iteration) =>
			{
				Server.SendBroadcast($"{remaining} seconds left", 2, Broadcast.BroadcastFlags.Normal, true);
			},
			onFinish: () =>
			{
				Server.SendBroadcast($"Zombies are released!", 10, Broadcast.BroadcastFlags.Normal, true);
				// start main event logic
			}
		);
	}

	private void SpawnPlayers()
	{
		// This is very dirty, but I have no idea how to properly find a spawnpoint for SCP0492. This spawns a probe player as SCP049, gets its spawnpoint and then changes it to SCP0492.
		Vector3 scp049Spawn;
		Player firstSeeker = _seekers.First();
		firstSeeker.SetRole(RoleTypeId.Scp049, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
		Timing.CallDelayed(0.2f, () =>
		{
			scp049Spawn = firstSeeker.Position;
			firstSeeker.SetRole(RoleTypeId.Scp0492, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
			foreach (Player z in _seekers.Skip(1))
			{
				z.SetRole(RoleTypeId.Scp0492, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
				z.Position = scp049Spawn + UnityEngine.Random.insideUnitSphere * 1.5f;
			}
			Logger.Debug($"Spawned {_seekers.Count} zombies at {scp049Spawn}");
		});
		foreach (Player hider in _hiders)
		{
			hider.SetRole(RoleTypeId.ClassD, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
			hider.Inventory.ServerAddItem(ItemType.GunCOM18, ItemAddReason.AdminCommand);
			hider.Inventory.ServerAddItem(ItemType.Lantern, ItemAddReason.AdminCommand);
			hider.Inventory.ServerAddAmmo(ItemType.Ammo9x19, 3);
		}
	}



}