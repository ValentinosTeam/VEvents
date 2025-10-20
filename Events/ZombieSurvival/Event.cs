using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomPlayerEffects;
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
	private List<Door> _lockedDoors;

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

		Utils.AddHandler(Timing.RunCoroutine(EventStartup()));
	}

	private IEnumerator<float> EventStartup()
	{
		// Wait until the round is started
		yield return Timing.WaitUntilTrue(() => Round.IsRoundStarted);
		Server.ClearBroadcasts();
		Logger.Debug("Round started, starting event...");
		Utils.CurrentState = State.Starting;
		GetZombieChamberDoors();
		GetForeverLockedDoors();
		GetZombieSpawn();
		RoundUtils.LockRound();
		MapUtils.FixAllDoors();
		MapUtils.CloseAllDoors();
		MapUtils.LockAllDoors();
		Map.SetColorOfLights(new Color(0.8f, 0.8f, 0.8f));
		PlayerUtils.SplitIntoTwoTeams(out Utils.Zombies, out Utils.Survivors, Config.ZombieRatio);
		Cassie.Clear();
		Utils.AddHandler(Timing.CallDelayed(2f, () =>
		{
			Cassie.Message(
				"Caution . biological contamination detected in the facility . initializing containment lockdown protocols . . . Main escapes are pitch_0.90 sealed pitch_0.85 off . .",
				false,
				true,
				true,
				"Caution! Biological contamination detected in the facility. Initializing containment lockdown protocols... Main escapes are sealed off!"
				);
		}));
		SpawnPlayers();
		Utils.AddHandler(CooldownUtils.Start(
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
	private void GetForeverLockedDoors()
	{
		_lockedDoors =
		[
			Door.Get("GATE_A"),
			Door.Get("GATE_B"),
			Door.Get("079_FIRST"), // TODO: in the future make a patch that allows listening for overcharge and alter the default behaviour.
			Door.Get("079_SECOND"),
			Door.Get("079_ARMORY")
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
		Utils.AddHandler(Timing.CallDelayed(0.2f, () =>
		{
			Utils.ZombieSpawn = probe.Position;
			Logger.Debug(Utils.ZombieSpawn);
			PlayerUtils.PlayersToSpectators();
		}));
	}
	private void SpawnPlayers()
	{
		Utils.AddHandler(Timing.CallDelayed(1f, () =>
		{
			foreach (Player zombie in Utils.Zombies)
			{
				Utils.SpawnAsZombie(zombie);
				zombie.MaxHealth = 600f; // The first zombies are slightly stronger
				zombie.Health = 600f;
				Item item = zombie.AddItem(ItemType.KeycardChaosInsurgency);
				zombie.CurrentItem = item;
			}
			foreach (Player survivor in Utils.Survivors)
			{
				Utils.SpawnAsSurvivor(survivor);
			}
		}));
	}
	private void ReleaseSurvivors()
	{
		MapUtils.OpenAllDoors(exceptions: _zombieChamberDoors.Concat(_lockedDoors).ToList()); // Keep the zombies locked in SCP-049 chamber. Hcz049Armory
		foreach (Door door in Room.Get(RoomName.LczClassDSpawn).First().Doors) door.IsOpened = true; // Open the Class D spawn doors
		Utils.CurrentState = State.SurvivorsReleased;
		Utils.AddHandler(Timing.CallDelayed(Config.ZombieReleaseDelay/2, () =>
		{
			Map.TurnOffLights();
			Utils.AddHandler(Timing.CallDelayed(1f, () =>
			{
				Map.TurnOnLights();
				Map.SetColorOfLights(new Color(0.7f, 0.2f, 0.2f));
			}));
			Cassie.Clear();
			Cassie.Message(
				"jam_04_3 Warning . . jam_01_1 Power yield_0.3 System jam_01_4 Critical . . This is an jam_1_3 emergency . . Light and door controls jam_01_4 are shutting pitch_0.80 down pitch_1 . All pitch_1.1 personnel pitch_0.9 take pitch_1 shelter pitch_0.8 immediately . .",
				false,
				true,
				true,
				"W-W-Warning! P-Power System C-C-C-Critical. This is an e-e-emergency! Light and Door controls a-a-a-are shutting d o w n. All PERSONNEL t a k e shelter i m m e d i a t e l y..."
			);
		}));
		Utils.AddHandler(Timing.CallDelayed(
			Config.ZombieReleaseDelay - 20,
			() =>
			{
				Map.TurnOffLights();
				Utils.AddHandler(Timing.CallDelayed(1f, () =>
				{
					Map.TurnOnLights();
					Map.SetColorOfLights(new Color(0.5f, 0f, 0f));
				}));
				Cassie.Clear();
				Cassie.Message(
					"pitch_1 bell_start .G6 jam_01_5 Alert . .G4 . jam_20_5 Power .G1 System jam_05_3 Failure . jam_30_2 Containment . unit .G3 pitch_0.9 breached . jam_02_3 All jam_50_4 personnel .G1 .G2 . jam_10_9 evacuate .G4 . . . jam_20_3 pitch_0.6 System . pitch_0.4 offline pitch_0.1 .G7 .",
					false,
					false,
					true,
					"\u26a0 A–A–A–Alert. Pow\u0337-pow\u0334-Power S\u0334ystem F\u0360a-f\u0337ail-Failure. Containment u\u0337-u\u0336-unit breached. A-a-All per\u0335-per-Personnel e\u0334-eva-Evacuate. S\u0336y-s\u035ey-System o\u0362ffline…"
				);
			}
		));
		Utils.AddHandler(CooldownUtils.Start(
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
		Map.TurnOffLights();
		MapUtils.OpenDoors(_zombieChamberDoors);
		Utils.CurrentState = State.ZombiesReleased;
		Utils.AddHandler(CooldownUtils.Start(
			key: "ZombieSurvivalMainTimer",
			duration: Config.EventDuration,
			interval: 1f,
			delay: 0f,
			onInterval: (remaining, iteration) =>
			{
				Server.SendBroadcast(Config.TimeUntilEventEndsMessage.Replace("{0}", remaining.ToString()), 2, Broadcast.BroadcastFlags.Normal, true);
			},
			onFinish: () => Utils.CurrentState = State.Ended
		));
		Utils.AddHandler(Timing.RunCoroutine(WaitForEventEnd()));
		foreach (Player zombie in Utils.Zombies) zombie.RemoveItem(ItemType.KeycardChaosInsurgency); // remove snake from their inventory so they dont just play snake all game
	}

	private IEnumerator<float> WaitForEventEnd()
	{
		CoroutineHandle randomEventsHandle = Timing.RunCoroutine(RandomEventsWhileWaiting());
		Utils.AddHandler(randomEventsHandle);
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
				Utils.AddHandler(Timing.CallDelayed(1f, Map.TurnOffLights));
				break;
			case SubEvent.BackupPower:
				Logger.Debug("Turning on temporary backup power");
				Map.TurnOnLights();
				Cassie.Message("turning pitch_0.7 on pitch_1 backup pitch_1.1 jam_001_2 power", false, false, true, "T-T-TURNING ON BACKUP P-POWER.");
				MapUtils.UnlockAllDoors(exceptions: _zombieChamberDoors.Concat(_lockedDoors).ToList());
				Utils.PowerIs = PowerIs.On;
				Utils.AddHandler(Timing.CallDelayed(Random.Range(10f, 30f), () =>
				{
					Map.TurnOffLights();
					Cassie.Message("jam_1_3 backup yield_0.5 pitch_0.8 power yield_0.6 jam_2_2 pitch_0.5 out pitch_0.10 .G5 ", false, false, true, "B-B-BACKUP P O W E R  o...u...t....");
					MapUtils.OpenAllDoors(exceptions: _zombieChamberDoors.Concat(_lockedDoors).ToList());
					MapUtils.LockAllDoors(exceptions: _zombieChamberDoors.Concat(_lockedDoors).ToList());
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

		Utils.AddHandler(CooldownUtils.Start(
			duration: Config.EndEventDuration,
			interval: Config.EndEventDuration,
			onInterval: (remaining, iteration) => { },
			onFinish: Stop
		));

	}

	protected override void OnStop()
	{
		CooldownUtils.Stop("ZombieSurvivalMainTimer");
		Utils.KillHandlers();
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