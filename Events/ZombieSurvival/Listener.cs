using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp079Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace VEvents.Events.ZombieSurvival;

internal class Listener(Config settings, Utils utils) : CustomEventsHandler
{
	private ZombieSurvival.Config Settings { get; set; } = settings;

	public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
	{
		switch (utils.CurrentState)
		{
			case State.Starting:
			case State.SurvivorsReleased:
				if (Random.value < Settings.ZombieRatio) utils.SpawnAsZombie(ev.Player);
				else utils.SpawnAsSurvivor(ev.Player);
				break;
			case State.ZombiesReleased:
				utils.SpawnAsZombie(ev.Player);
				break;
			case State.Ended:
			case State.PreRound:
			default:
				break;
		}
	}

	public override void OnPlayerDeath(PlayerDeathEventArgs ev)
	{
		switch (utils.CurrentState)
		{
			case State.SurvivorsReleased:
			case State.ZombiesReleased:
				if (utils.Zombies.Contains(ev.Attacker)) utils.SpawnAsZombie(ev.Player, false);
				else utils.SpawnAsZombie(ev.Player, true);
				if (utils.ZombiesWonEarly()) utils.CurrentState = State.Ended;
				break;
			case State.Ended:
			case State.PreRound:
			case State.Starting:
			default:
				Logger.Warn($"Player {ev.Player.Nickname} somehow called OnPlayerDeath in state {utils.CurrentState}. It should be impossible.");
				break;
		}
	}

	public override void OnPlayerLeft(PlayerLeftEventArgs ev)
	{
		switch (utils.CurrentState)
		{
			case State.Starting:
			case State.SurvivorsReleased:
			case State.ZombiesReleased:
				utils.RemovePlayer(ev.Player);
				if (utils.ZombiesWonEarly()) utils.CurrentState = State.Ended;
				break;
			case State.PreRound:
			case State.Ended:
			default:
				break;
		}
	}

	public override void OnServerCassieQueuingScpTermination(CassieQueuingScpTerminationEventArgs ev)
	{
		ev.IsAllowed = false;
	}

	public override void OnServerWaveRespawning(WaveRespawningEventArgs ev)
	{
		ev.IsAllowed = false;
	}
}