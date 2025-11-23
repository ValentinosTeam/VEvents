using LabApi.Features.Wrappers;

namespace VEvents.Helpers;

public static class RoundUtils
{
	public static void LockRound()
	{
		Round.IsLocked = true;
	}
	public static void UnlockRound()
	{
		Round.IsLocked = false;
	}

	public static void TurnFFOn()
	{
		Server.FriendlyFire = true;
	}

	public static void TurnFFOff()
	{
		Server.FriendlyFire = false;
	}

}