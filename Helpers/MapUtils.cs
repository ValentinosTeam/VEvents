using System.Collections.Generic;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace VEvents.Helpers;

public static class MapUtils
{

	public static void FixAllDoors()
	{
		foreach (Door door in Door.List)
		{
			if (door is BreakableDoor breakableDoor) breakableDoor.TryRepair();
		}
	}

	public static void LockAllDoors()
	{
		Logger.Debug("Locking all doors.");
		foreach (Door door in Door.List)
		{
			if (door is not BreakableDoor && door is not Gate) continue;
			door.IsLocked = true;
		}
	}
	public static void CloseAllDoors()
	{
		Logger.Debug("Closing and locking all doors.");
		foreach (Door door in Door.List)
		{
			if (door is not BreakableDoor && door is not Gate) continue;
			door.IsOpened = false;
		}
	}

	public static void OpenAllDoors(List<Door> exceptions = null)
	{
		Logger.Debug("Unlocking all doors...");
		foreach (Door door in Door.List)
		{
			if (exceptions != null && exceptions.Contains(door)) continue;
			if (door is not BreakableDoor && door is not Gate) continue;
			door.IsOpened = true;
		}
	}

	public static void LockAllDoors(List<Door> exceptions = null)
	{
		Logger.Debug("Opening and locking all doors...");
		foreach (Door door in Door.List)
		{
			if (exceptions != null && exceptions.Contains(door)) continue;
			if (door is not BreakableDoor && door is not Gate) continue;
			door.IsLocked = true;
		}
	}

	public static void UnlockAllDoors(List<Door> exceptions = null)
	{
		Logger.Debug("Unlocking all doors...");
		foreach (Door door in Door.List)
		{
			if (exceptions != null && exceptions.Contains(door)) continue;
			if (door is not BreakableDoor && door is not Gate) continue;
			door.IsLocked = false;
		}
	}

	public static void OpenDoors(List<Door> doors)
	{
		foreach (Door door in doors)
		{
			door.IsOpened = true;
		}
	}
}