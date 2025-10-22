using System.Reflection;
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using PlayerRoles.PlayableScps.Scp079;

namespace VEvents.Extensions;

[HarmonyPatch(typeof(Scp079Recontainer))]
[HarmonyPatch("Start")]
public class Scp079RecontainerPatch : CustomEventsHandler
{
	public static BreakableWindow Scp079Glass = null!;

	public static void Prefix(Scp079Recontainer __instance)
	{
		Logger.Debug("Started getting the glass");
		FieldInfo field = AccessTools.Field(typeof(Scp079Recontainer), "_activatorGlass");
		Scp079Glass = (BreakableWindow)field.GetValue(__instance);
	}
}