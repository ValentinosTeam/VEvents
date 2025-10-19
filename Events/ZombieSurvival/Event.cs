using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomPlayerEffects;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.ThrowableProjectiles;
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

	private List<CoroutineHandle> _handles = [];

	private Utils Utils { get; set; }
	private Listener Listener { get; set; }

	protected override void OnStart()
	{
		Logger.Debug("Starting event...");
		Utils = new Utils(Config);
		Utils.CurrentState = State.PreRound;

		Listener = new Listener(Config, Utils);
		CustomHandlersManager.RegisterEventsHandler(Listener);
		Server.SendBroadcast(Config.EventStartingMessage, 60);

		_handles.Add(Timing.RunCoroutine(EventStartup()));
	}

	private IEnumerator<float> EventStartup()
	{
		// Wait until the round is started
		yield return Timing.WaitUntilTrue(() => Round.IsRoundStarted);
		Server.ClearBroadcasts();
		Logger.Debug("Round started, starting event...");
		Utils.CurrentState = State.Starting;
		GetZombieChamberDoors();
		GetSurfaceGates();
		GetZombieSpawn();
		RoundUtils.LockRound();
		MapUtils.FixAllDoors();
		MapUtils.CloseAllDoors();
		MapUtils.LockAllDoors();
		Map.SetColorOfLights(new Color(0.3f, 0.8f, 0.3f));
		PlayerUtils.SplitIntoTwoTeams(out Utils.Zombies, out Utils.Survivors, Config.ZombieRatio);
		SpawnPlayers();
		_handles.Add(CooldownUtils.Start(
			duration: Config.GuideMessageInterval * Config.ZombieGuideMessages.Count,
			interval: Config.GuideMessageInterval,
			onInterval: (remaining, iteration) =>
			{
				foreach (Player zombie in Utils.Zombies)
				{
					Server.SendBroadcast(zombie, Config.ZombieGuideMessages[iteration], 11, Broadcast.BroadcastFlags.Normal, true);
				}
				foreach (Player survivor in Utils.Survivors)
				{
					Server.SendBroadcast(survivor, Config.SurvivorGuideMessages[iteration], 11, Broadcast.BroadcastFlags.Normal, true);
				}
			},
			onFinish: ReleaseSurvivors
		));
	}
	private void GetZombieChamberDoors()
	{
		Door door = Door.Get("049_ARMORY");
		Room room = door?.Rooms.First();
		_zombieChamberDoors = [];
		if (room == null) return;
		foreach (Door d in room.Doors)
		{
			if ((d.GetType() == typeof(Gate) && Mathf.Abs(d.Position.y - door.Position.y) <= 1) || d == door)
			{
				_zombieChamberDoors.Add(d);
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
		_handles.Add(Timing.CallDelayed(0.2f, () =>
		{
			Utils.ZombieSpawn = probe.Position;
			Logger.Debug(Utils.ZombieSpawn);
			PlayerUtils.PlayersToSpectators();
		}));
	}
	private void SpawnPlayers()
	{
		_handles.Add(Timing.CallDelayed(1f, () =>
		{
			foreach (Player zombie in Utils.Zombies)
			{
				Utils.SpawnAsZombie(zombie);
				zombie.MaxHealth = 600f; // The first zombies are slightly stronger
				zombie.Health = 600f;
			}
			foreach (Player survivor in Utils.Survivors)
			{
				Utils.SpawnAsSurvivor(survivor);
			}
		}));
	}
	private void ReleaseSurvivors()
	{
		MapUtils.OpenAllDoors(exceptions: _zombieChamberDoors.Concat(_surfaceGates).ToList()); // Keep the zombies locked in SCP-049 chamber. Hcz049Armory
		foreach (Door door in Room.Get(RoomName.LczClassDSpawn).First().Doors) door.IsOpened = true; // Open the Class D spawn doors
		Utils.CurrentState = State.SurvivorsReleased;
		_handles.Add(CooldownUtils.Start(
			duration: Config.ZombieReleaseDelay,
			interval: 1f,
			onInterval: (remaining, iteration) =>
			{
				Server.SendBroadcast(Config.TimeUntilZombiesReleasedMessage.Replace("{0}", remaining.ToString()), 2, Broadcast.BroadcastFlags.Normal, true);
			},
			onFinish: ReleaseZombies
		));
	}
	private void ReleaseZombies()
	{
		const int broadcastDuration = 10;
		Server.SendBroadcast($"Zombies are released!", broadcastDuration, Broadcast.BroadcastFlags.Normal, true);
		Map.TurnOffLights();
		MapUtils.OpenDoors(_zombieChamberDoors);
		Utils.CurrentState = State.ZombiesReleased;
		_handles.Add(CooldownUtils.Start(
			key: "ZombieSurvivalMainTimer",
			duration: Config.EventDuration - broadcastDuration,
			interval: 1f,
			delay: broadcastDuration,
			onInterval: (remaining, iteration) =>
			{
				Server.SendBroadcast(Config.TimeUntilEventEndsMessage.Replace("{0}", remaining.ToString()), 2, Broadcast.BroadcastFlags.Normal, true);
			},
			onFinish: () => Utils.CurrentState = State.Ended
		));
		_handles.Add(Timing.RunCoroutine(WaitForEventEnd()));
	}

	private IEnumerator<float> WaitForEventEnd()
	{
		CoroutineHandle randomEventsHandle = Timing.RunCoroutine(RandomEventsWhileWaiting());
		_handles.Add(randomEventsHandle);
		yield return Timing.WaitUntilTrue(() => Utils.CurrentState == State.Ended);
		Timing.KillCoroutines(randomEventsHandle);
		EndEvent();
	}
	private IEnumerator<float> RandomEventsWhileWaiting()
	{
		while (true)
		{
			//TODO: make configurable
			float delay = Random.Range(Config.SubEventMinInterval, Config.SubEventMaxInterval);
			yield return Timing.WaitForSeconds(delay);

			if (Utils.CurrentState == State.Ended)
				yield break;

			Logger.Debug("Triggering random sub-event...");
			TriggerRandomSubEvent();
		}
	}
	private void TriggerRandomSubEvent()
	{
		int totalWeight = Config.SubEventWeights.Values.Sum();

		int roll = Random.Range(0, totalWeight);

		var selectedEvent = SubEvent.None;
		foreach (var kvp in Config.SubEventWeights)
		{
			roll -= kvp.Value;
			if (roll < 0)
			{
				selectedEvent = kvp.Key;
				break;
			}
		}

		switch (selectedEvent)
		{
			case SubEvent.None:
			default:
				Logger.Debug("Doing nothing as a random event.");
				break;
			case SubEvent.Cassie:
				int randomCassieMessage = Random.Range(0, 7);
				Cassie.Clear();
				Logger.Debug($"Playing glitchy cassie message as a random event. pitch_0.10 .G{randomCassieMessage}");
				Cassie.Message($"pitch_0.10 .G{randomCassieMessage}", false, false, false);
				break;
			case SubEvent.Amnesia:
				Logger.Debug("Giving amnesia effect to all survivors as a random event.");
				foreach (Player survivor in Utils.Survivors) survivor.EnableEffect<AmnesiaVision>(1, Random.Range(10f, 30f));
				break;
			case SubEvent.Flicker:
				Logger.Debug("Flickering lights as a random event.");
				Map.TurnOnLights();
				_handles.Add(Timing.CallDelayed(1f, Map.TurnOffLights));
				break;
			case SubEvent.BackupPower:
				Logger.Debug("Turning on temporary backup power");
				Map.TurnOnLights();
				Cassie.Message("turning pitch_0.7 on pitch_1 backup pitch_1.1 jam_001_2 power", false, false, true, "T-T-TURNING ON BACKUP P-POWER.");
				MapUtils.UnlockAllDoors(exceptions: _zombieChamberDoors.Concat(_surfaceGates).ToList());
				Utils.PowerIs = PowerIs.On;
				_handles.Add(Timing.CallDelayed(Random.Range(10f, 30f), () =>
				{
					Map.TurnOffLights();
					Cassie.Message("jam_1_3 backup yield_0.5 pitch_0.8 power yield_0.6 jam_2_2 pitch_0.5 out pitch_0.10 .G5 ", false, false, true, "B-B-BACKUP P O W E R  o...u...t....");
					MapUtils.OpenAllDoors(exceptions: _zombieChamberDoors.Concat(_surfaceGates).ToList());
					MapUtils.LockAllDoors(exceptions: _zombieChamberDoors.Concat(_surfaceGates).ToList());
					Utils.PowerIs = PowerIs.Off;
				}));
				break;
		}
	}

	private void EndEvent()
	{
		CooldownUtils.Stop("ZombieSurvivalMainTimer");
		Map.TurnOnLights();
		if (!Utils.Survivors.IsEmpty())
		{
			Map.SetColorOfLights(Color.green);
			var survivorNames = new StringBuilder();
			PlayerUtils.PlayersToSpectators(Utils.Survivors);
			foreach (Player survivor in Utils.Survivors.Where(h => h.IsAlive))
			{
				survivorNames.Append(survivor.Nickname).Append(", ");
				survivor.SetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
			}
			survivorNames.Length -= 2;  // Remove last ", "
			Server.SendBroadcast(Config.SurvivorsWinMessage
					.Replace("{0}", Utils.Survivors.Count.ToString())
					.Replace("{1}", survivorNames.ToString()), (ushort)Config.EndEventDuration, Broadcast.BroadcastFlags.Normal, true);
		}
		else
		{
			Map.SetColorOfLights(Color.red);
			Server.SendBroadcast(Config.ZombiesWinMessage, (ushort)Config.EndEventDuration, Broadcast.BroadcastFlags.Normal, true);
			PlayerUtils.PlayersToSpectators(Utils.Zombies);
			foreach (Player zombie in Utils.Zombies)
			{
				zombie.SetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
			}
		}

		_handles.Add(CooldownUtils.Start(
			duration: Config.EndEventDuration,
			interval: Config.EndEventDuration,
			onInterval: (remaining, iteration) => { },
			onFinish: Stop
		));

	}

	protected override void OnStop()
	{
		CooldownUtils.Stop("ZombieSurvivalMainTimer");
		foreach (CoroutineHandle handle in _handles)
		{
			Timing.KillCoroutines(handle);
		}
		_handles.Clear();
		PlayerUtils.PlayersToSpectators();
		Map.ResetColorOfLights();
		Map.TurnOnLights();
		MapUtils.FixAllDoors();
		MapUtils.UnlockAllDoors();
		MapUtils.CloseAllDoors();
		Server.ClearBroadcasts();
		Cassie.Clear();

		CustomHandlersManager.UnregisterEventsHandler(Listener);
		Utils = null;
		Listener = null;
		Server.SendBroadcast("Zombie Survival event has ended.", 10);
	}

	public override bool CanStartManually(out string response)
	{
		Logger.Debug("Checking if event can be started manually...");
		//TODO: Make sure no other event is running.
		response = null;
		if (!Config.IsEnabled)
		{
			response = "Not enabled";
			return false;
		}

		if (!Config.IsManual)
		{
			response = "The event can't be started manually";
			return false;
		}

		if (Round.IsRoundEnded)
		{
			response = "Round has already ended";
			return false;
		}

		if (Player.List.Count < Config.MinPlayers)
		{
			response = $"Not enough players, min is {Config.MinPlayers}";
			return false;
		}
		Logger.Debug("Event can be started manually.");
		response = "Starting successfully";
		return true;
	}

}