using System.Collections.Generic;
using System.Linq;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MEC;
using VEvents.Core;
using VEvents.Helpers;
using Logger = LabApi.Features.Console.Logger;

namespace VEvents.Events.Test;

public class Event : EventBase<Config>
{
	public override string Name { get; } = "Test";
	public override string Description { get; } = "This is a test event for development purposes.";

	private Utils Utils { get; set; }
	private Listener Listener { get; set; }

	private int _testCounter = 0;

	protected override void OnStart()
	{
		Logger.Debug("Starting event...");
		Utils = new Utils();
		Listener = new Listener();
		CustomHandlersManager.RegisterEventsHandler(Listener);

		Utils.AddHandler(Timing.RunCoroutine(EventStartup()));
	}

	private IEnumerator<float> EventStartup()
	{
		// Wait until the round is started
		yield return Timing.WaitUntilTrue(() => Round.IsRoundStarted);
		while (true)
		{
			yield return Timing.WaitForSeconds(1f); // check again in 1 second
			List<Player> checkedPlayers = [];
			bool allReady = true;
			foreach (Player player in Player.List)
			{
				if (player.IsHost) continue;
				if (checkedPlayers.Contains(player))
				{
					Logger.Warn($"Player {player.Nickname} is a duplicate!");
				}
				else
				{
					checkedPlayers.Add(player);
				}
				if (player.IsReady) continue;
				allReady = false;
				checkedPlayers.Clear();
				Logger.Warn($"Player {player.Nickname} is not ready yet. waiting...");
			}
			if (allReady) break;
		}
		Logger.Debug("All players are ready, starting event...");
		//yield return Timing.WaitForSeconds(5f);
		// Logger.Debug("Round started, starting event...");
		List<Player> group1 = new List<Player>();
		List<Player> group2 = new List<Player>();
		PlayerUtils.SplitIntoTwoTeams(out group1, out group2, 1f/6f);
		Logger.Debug("Done splitting players into two groups.");
		Logger.Debug($"group1 count: {group1.Count}, group2 count: {group2.Count}, the sum is {group1.Count + group2.Count}, total players: {Player.List.Count}");

		Utils.AddHandler(CooldownUtils.Start(
			key: "ZombieSurvivalMainTimer",
			duration: 1000f,
			interval: 1f,
			delay: 1f,
			onInterval: OnMainTimerTick,
			onFinish: () => Logger.Debug("Done")
		));
	}

	private void OnMainTimerTick(float remaining, int iteration)
	{
		_testCounter++;
		if (iteration % 2 != 0) return; // Apply debuffs only every 2 ticks

		foreach (Player survivor in Player.List)
		{
			if (survivor.Room == null) continue;
			int othersNearby = survivor.Room.Players.Count(p => p != survivor);
			if (othersNearby <= 2) continue;

			ApplyDebuffs(survivor, othersNearby);
		}
		Logger.Info($"Test complete: total triggers = {_testCounter}");
		if (_testCounter - 100 >= int.MaxValue) _testCounter = 0;
	}
	private void ApplyDebuffs(Player survivor, int nearbyCount)
	{
		if (nearbyCount > 2) _testCounter++;
		if (nearbyCount > 4) _testCounter++;
		if (nearbyCount > 6) _testCounter++;
		if (nearbyCount > 8) _testCounter++;
		if (nearbyCount > 10) _testCounter++;
	}

	protected override void OnStop()
	{
		Utils.KillHandlers();

		CustomHandlersManager.UnregisterEventsHandler(Listener);
		Utils = null;
		Listener = null;
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