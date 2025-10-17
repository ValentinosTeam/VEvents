using System.Collections.Generic;
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

	internal Utils()
	{
		Survivors = [];
		Zombies = [];
		ZombieSpawn = Vector3.zero;
		CurrentState = State.PreRound;
	}

	internal void SpawnAsZombie(Player player)
	{
		if (Survivors.Contains(player)) Survivors.Remove(player);
		if (!Zombies.Contains(player)) Zombies.Add(player);
		player.SetRole(RoleTypeId.Scp0492, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
		Logger.Debug($"{player.Nickname} is now a zombie");
		player.Inventory.ServerAddItem(RandomZombieItem(), ItemAddReason.AdminCommand);
		player.Position = ZombieSpawn;
	}
	private ItemType RandomZombieItem()
	{
		return ItemType.Ammo9x19;
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