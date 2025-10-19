using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using InventorySystem;
using InventorySystem.Items;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp939.Ripples;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace VEvents.Events.ZombieSurvival;

internal class Utils
{
	internal List<Player> Survivors;
	internal List<Player> Zombies;
	internal Vector3 ZombieSpawn { get; set; }
	internal State CurrentState { get; set; }
	internal PowerIs PowerIs { get; set; }
	private Config Settings { get; set; }

	internal Utils(Config settings)
	{
		Survivors = [];
		Zombies = [];
		ZombieSpawn = Vector3.zero;
		CurrentState = State.PreRound;
		PowerIs = PowerIs.Off;
		Settings = settings;
	}

	internal void SpawnAsZombie(Player player, bool useZombieSpawn = true)
	{
		if (Survivors.Contains(player)) Survivors.Remove(player);
		if (!Zombies.Contains(player)) Zombies.Add(player);
		player.SetRole(RoleTypeId.Scp0492, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
		Logger.Debug($"{player.Nickname} is now a zombie");
		GenerateZombieLoot(player);
		if (useZombieSpawn) player.Position = ZombieSpawn;
		else
		{
			player.EnableEffect<Flashed>(1, 5f);
			player.EnableEffect<Deafened>(50, 10f);
			player.EnableEffect<Blurred>(25, 25f);
			player.EnableEffect<Blindness>(50, 25f);
			player.EnableEffect<AmnesiaVision>(1, 25f);
			player.EnableEffect<Slowness>(80, 25f);
		}

	}

	private void GenerateZombieLoot(Player player)
	{
		if (Settings.ZombieDrops == null || Settings.ZombieDrops.Count == 0) return;

		int randomIndex = UnityEngine.Random.Range(0, Settings.ZombieDrops.Count);
		KeyValuePair<ItemType, int> randomEntry = Settings.ZombieDrops[randomIndex].First();
		try
		{
			ItemType itemType = randomEntry.Key;
			int amount = randomEntry.Value;
			for (int i = 0; i < amount; i++) player.AddItem(itemType);
		}
		catch (Exception e)
		{
			Logger.Error($"Error generating zombie loot - {randomEntry.Key}: {randomEntry.Value}.\n" + e.Message);
		}
	}

	internal void SpawnAsSurvivor(Player player)
	{
		if (Zombies.Contains(player)) Zombies.Remove(player);
		if (!Survivors.Contains(player)) Survivors.Add(player);
		player.SetRole(RoleTypeId.ClassD, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.UseSpawnpoint);
		Logger.Debug($"{player.Nickname} is now a survivor");
		player.Inventory.ServerAddItem(ItemType.GunCOM18, ItemAddReason.AdminCommand);
		player.Inventory.ServerAddItem(ItemType.Lantern, ItemAddReason.AdminCommand);
		player.Inventory.ServerAddAmmo(ItemType.Ammo9x19, 3);
	}

	internal void RemovePlayer(Player player)
	{
		if (Zombies.Contains(player)) Zombies.Remove(player);
		if (Survivors.Contains(player)) Survivors.Remove(player);
	}

	internal bool ZombiesWonEarly()
	{
		return (Survivors.Count == 0);
	}
}