using System.Collections.Generic;
using MEC;
namespace VEvents.Events.Test;

internal class Utils
{
	private List<CoroutineHandle> CoroutineHandles { get; set; }

	internal Utils()
	{
		CoroutineHandles = [];
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