using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace VEvents.Helpers;

public static class PlayerUtils
{
	public static void PlayersToSpectators(List<Player> exceptions = null)
	{
		foreach (Player p in Player.List)
		{
			if (p.Role == RoleTypeId.Overwatch) continue;
			if (p.IsHost) continue;
			if (exceptions != null && exceptions.Contains(p)) continue;
			p.SetRole(RoleTypeId.Spectator, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.AssignInventory | RoleSpawnFlags.UseSpawnpoint);
		}
	}

	public static void SplitIntoTwoTeams(out List<Player> team1, out List<Player> team2, float ratio)
	{
		team1 = [];
		team2 = [];
		var players = Player.ReadyList.ToList();
		players = players.Where(p => p.Role != PlayerRoles.RoleTypeId.Overwatch && !p.IsHost).OrderBy(_ => Random.value).ToList();
		int totalPlayers = players.Count;
		int team1Count = Mathf.Clamp(Mathf.CeilToInt(totalPlayers * ratio), 1, totalPlayers - 1);
		team1 = players.Take(team1Count).ToList();
		team2 = players.Skip(team1Count).ToList();

		int i = 0;
		foreach (Player player in team1) Logger.Debug($"Player {i}: {player.Nickname} ({player.UserId}) is on team1");
		i = 0;
		foreach (Player player in team2) Logger.Debug($"Player {i}: {player.Nickname} ({player.UserId}) is on team2");
		Logger.Debug($"team1Count: {team1.Count} / {players.Count} players. team2Count: {team2.Count} / {players.Count} players.");
	}
}