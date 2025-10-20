using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using InventorySystem;
using InventorySystem.Items;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
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
	private List<CoroutineHandle> CoroutineHandles { get; set; }

	internal Utils(Config settings)
	{
		Survivors = [];
		Zombies = [];
		ZombieSpawn = Vector3.zero;
		CurrentState = State.PreRound;
		PowerIs = PowerIs.Off;
		Settings = settings;
		CoroutineHandles = new List<CoroutineHandle>();
	}

	internal void SpawnAsZombie(Player player, bool useZombieSpawn = true)
	{
		if (Survivors.Contains(player)) Survivors.Remove(player);
		if (!Zombies.Contains(player)) Zombies.Add(player);
		player.SetRole(RoleTypeId.Scp0492, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
		player.Health = 300;
		player.MaxHealth = 300;
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
		player.Health = 160;
		player.MaxHealth = 160;
		foreach (var item in Settings.SurvivorSpawnItems)
		{
			ItemType itemType = item.First().Key;
			int itemAmount = item.First().Value;
			for (int i = 0; i < itemAmount; i++) player.Inventory.ServerAddItem(itemType, ItemAddReason.AdminCommand);
		}
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

	internal void AddHandler(CoroutineHandle handle)
	{
		CoroutineHandles.Add(handle);
	}

	internal void KillHandlers()
	{
		foreach (CoroutineHandle handle in CoroutineHandles) Timing.KillCoroutines(handle);
		CoroutineHandles.Clear();
	}
}