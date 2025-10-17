using System.Collections.Generic;
using System.Linq;
using System.Text;
using InventorySystem;
using InventorySystem.Items;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using UnityEngine;
using VEvents.Core;
using VEvents.Helpers;
using Logger = LabApi.Features.Console.Logger;
using Random = UnityEngine.Random;

namespace VEvents.Events.ZombieSurvival;

public class Event : EventBase<Config>
{
	public override string Name { get; } = "ZombieSurvival";
	public override string Description { get; } = "Event turns off the lights and makes 2 teams, Survivors and Zombies. Zombies have to find Survivors and kill them to convert them to their team. Last Survivor standing at the end of the event wins.";

	private List<Door> _zombieChamberDoors;
	private List<Door> _surfaceGates;

	private Utils _utils;
	private Listener _listener;

	protected override void OnStart()
	{
		Logger.Debug("Starting event...");
		_utils = new Utils();
		_utils.CurrentState = State.PreRound;

		_listener = new Listener(Settings, _utils);
		CustomHandlersManager.RegisterEventsHandler(_listener);
		Server.SendBroadcast(Settings.EventStartingMessage, 60);

		Timing.RunCoroutine(EventStartup());
	}

	private IEnumerator<float> EventStartup()
	{
		// Wait until the round is started
		yield return Timing.WaitUntilTrue(() => Round.IsRoundStarted);
		Server.ClearBroadcasts();
		Logger.Debug("Round started, starting event...");
		_utils.CurrentState = State.Starting;
		GetZombieChamberDoors();
		GetSurfaceGates();
		GetZombieSpawn();
		RoundUtils.LockRound();
		MapUtils.CloseAndLockAllDoors();
		PlayerUtils.SplitIntoTwoTeams(out _utils.Zombies, out _utils.Survivors, Settings.ZombieRatio);
		SpawnPlayers();
		CooldownUtils.Start(
			duration: Settings.GuideMessageInterval * Settings.ZombieGuideMessages.Count,
			interval: Settings.GuideMessageInterval,
			onInterval: (remaining, iteration) =>
			{
				foreach (Player zombie in _utils.Zombies)
				{
					Server.SendBroadcast(zombie, Settings.ZombieGuideMessages[iteration], 11, Broadcast.BroadcastFlags.Normal, true);
				}
				foreach (Player survivor in _utils.Survivors)
				{
					Server.SendBroadcast(survivor, Settings.SurvivorGuideMessages[iteration], 11, Broadcast.BroadcastFlags.Normal, true);
				}
			},
			onFinish: ReleaseSurvivors
		);
	}
	private void GetZombieChamberDoors()
	{
		Door door = Door.Get("049_ARMORY");
		Room room = door?.Rooms.First();
		_zombieChamberDoors = [];
		if (room != null)
		{
			foreach (Door d in room.Doors)
			{
				if ((d.GetType() == typeof(Gate) && Mathf.Abs(d.Position.y - door.Position.y) <= 1) || d == door)
				{
					_zombieChamberDoors.Add(d);
				}
			}
		}
	}
	private void GetSurfaceGates()
	{
		_surfaceGates =
		[
			Door.Get("GATE_A"),
			Door.Get("GATE_B")
		];
	}
	private void GetZombieSpawn()
	{
		// This is very dirty, but I have no idea how to properly find a spawnpoint for SCP0492. This spawns a probe player as SCP049, gets its spawnpoint and then changes it to SCP0492.
		List<Player> candidates = Player.List
			.Where(p => p.Role != RoleTypeId.Overwatch && !p.IsHost)
			.OrderBy(_ => Random.value)
			.ToList();
		Player probe = candidates.First();
		probe.SetRole(RoleTypeId.Scp049, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
		Logger.Debug("Probe is now Doctor");
		Timing.CallDelayed(0.2f, () =>
		{
			_utils.ZombieSpawn = probe.Position;
			Logger.Debug(_utils.ZombieSpawn);
			PlayerUtils.PlayersToSpectators();
		});
	}
	private void SpawnPlayers()
	{
		Timing.CallDelayed(1f, () =>
		{
			foreach (Player zombie in _utils.Zombies)
			{
				_utils.SpawnAsZombie(zombie);
			}
			foreach (Player survivor in _utils.Survivors)
			{
				_utils.SpawnAsSurvivor(survivor);
			}
		});
	}
	private void ReleaseSurvivors()
	{
		MapUtils.UnlockAllDoors(exceptions: _zombieChamberDoors.Concat(_surfaceGates).ToList()); // Keep the zombies locked in SCP-049 chamber. Hcz049Armory
		foreach (Door door in Room.Get(RoomName.LczClassDSpawn).First().Doors) door.IsOpened = true; // Open the Class D spawn doors
		_utils.CurrentState = State.SurvivorsReleased;

		CooldownUtils.Start(
			duration: Settings.ZombieReleaseDelay,
			interval: 1f,
			onInterval: (remaining, iteration) =>
			{
				Server.SendBroadcast(Settings.TimeUntilZombiesReleasedMessage.Replace("{0}", remaining.ToString()), 2, Broadcast.BroadcastFlags.Normal, true);
			},
			onFinish: ReleaseZombies
		);
	}
	private void ReleaseZombies()
	{
		const int broadcastDuration = 10;
		Server.SendBroadcast($"Zombies are released!", broadcastDuration, Broadcast.BroadcastFlags.Normal, true);
		Map.TurnOffLights();
		MapUtils.OpenDoors(_zombieChamberDoors);
		_utils.CurrentState = State.ZombiesReleased;
		CooldownUtils.Start(
			key: "ZombieSurvivalMainTimer",
			duration: Settings.EventDuration - broadcastDuration,
			interval: 1f,
			delay: broadcastDuration,
			onInterval: (remaining, iteration) =>
			{
				Server.SendBroadcast(Settings.TimeUntilEventEndsMessage.Replace("{0}", remaining.ToString()), 2, Broadcast.BroadcastFlags.Normal, true);
			},
			onFinish: () => _utils.CurrentState = State.Ended
		);
		Timing.RunCoroutine(WaitForEventEnd());
	}

	private IEnumerator<float> WaitForEventEnd()
	{
		yield return Timing.WaitUntilTrue(() => _utils.CurrentState == State.Ended);
		EndEvent();
	}
	private void EndEvent()
	{
		CooldownUtils.Stop("ZombieSurvivalMainTimer");
		Map.TurnOnLights();
		if (!_utils.Survivors.IsEmpty())
		{
			Map.SetColorOfLights(Color.green);
			var survivorNames = new StringBuilder();
			PlayerUtils.PlayersToSpectators(_utils.Survivors);
			foreach (Player survivor in _utils.Survivors.Where(h => h.IsAlive))
			{
				survivorNames.Append(survivor.Nickname).Append(", ");
				survivor.SetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
			}
			survivorNames.Length -= 2;  // Remove last ", "
			Server.SendBroadcast(Settings.SurvivorsWinMessage
					.Replace("{0}", _utils.Survivors.Count.ToString())
					.Replace("{1}", survivorNames.ToString()), (ushort)Settings.EndEventDuration, Broadcast.BroadcastFlags.Normal, true);
		}
		else
		{
			Map.SetColorOfLights(Color.red);
			Server.SendBroadcast(Settings.ZombiesWinMessage, (ushort)Settings.EndEventDuration, Broadcast.BroadcastFlags.Normal, true);
			PlayerUtils.PlayersToSpectators(_utils.Zombies);
			foreach (Player zombie in _utils.Zombies)
			{
				zombie.SetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
			}
		}

		CooldownUtils.Start(
			duration: Settings.EndEventDuration,
			interval: Settings.EndEventDuration,
			onInterval: (remaining, iteration) => { },
			onFinish: Stop
		);

	}

	protected override void OnStop()
	{
		CooldownUtils.Stop("ZombieSurvivalMainTimer");
		RoundUtils.UnlockRound();
		PlayerUtils.PlayersToSpectators();
		CustomHandlersManager.UnregisterEventsHandler(_listener);
		_utils = null;
		_listener = null;
		Server.SendBroadcast("Zombie Survival event has ended.", 10);
	}

	public override bool CanStartManually()
	{
		Logger.Debug("Checking if event can be started manually...");
		//TODO: Make sure no other event is running.
		if (!Config.IsEnabled) return false;
		if (!Config.IsManual) return false;
		if (Round.IsRoundEnded) return false;
		if (IsRunning) return false;
		if (Player.List.Count < Settings.MinPlayers) return false;
		Logger.Debug("Event can be started manually.");
		return true;
	}
}