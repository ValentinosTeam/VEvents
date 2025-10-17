using LabApi.Features.Wrappers;

namespace VEvents.Helpers;

public class RoundUtils
{
	public static void LockRound()
	{
		Round.IsLocked = true;
	}
	public static void UnlockRound()
	{
		Round.IsLocked = false;
	}
}