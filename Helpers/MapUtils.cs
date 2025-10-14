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
			if (door is BreakableDoor breakableDoor) breakableDoor.TryRepair();
			door.IsOpened = false;
			door.IsLocked = true; // NOTE: Visual bug if a broken door got fixed and locked, the button doesn't show the locked state. Still works though.
		}
		Logger.Debug("All doors repaired, closed and locked.");
	}

	public static void UnlockAllDoors()
	{
		foreach (Door door in Door.List)
		{
			if (door is BreakableDoor breakableDoor) breakableDoor.TryRepair();
			door.IsLocked = false;
		}
	}
}