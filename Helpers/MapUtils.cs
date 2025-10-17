using System.Collections.Generic;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace VEvents.Helpers;

public class MapUtils
{
	public static void CloseAndLockAllDoors()
	{
		Logger.Debug("Preparing round: repairing, closing and locking all doors...");
		foreach (Door door in Door.List)
		{
			if (door is not BreakableDoor && door is not Gate) continue; // Only breakable doors and gates need to be repaired, closed and locked.
			if (door is BreakableDoor breakableDoor) breakableDoor.TryRepair();
			door.IsOpened = false;
			door.IsLocked = true; // NOTE: Visual bug if a broken door got fixed and locked, the button doesn't show the locked state. Still works though.
		}
		Logger.Debug("All doors repaired, closed and locked.");
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
		Logger.Debug("All doors unlocked.");
	}

	public static void OpenDoors(List<Door> doors)
	{
		foreach (Door door in doors)
		{
			door.IsOpened = true;
		}
	}
}