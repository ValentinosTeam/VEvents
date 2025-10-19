using System;
using System.Collections.Generic;
using MEC;

namespace VEvents.Helpers;

public static class CooldownUtils
{
	private static readonly Dictionary<string, CoroutineHandle> NamedCooldowns = new();
	private static readonly List<CoroutineHandle> UnnamedCooldowns = [];

	/// <summary>
	/// Starts a cooldown coroutine that runs an interval action repeatedly during the countdown, and executes a final action when finished.
	/// </summary>
	/// <param name="duration">Total countdown duration in seconds.</param>
	/// <param name="interval">How often to invoke <paramref name="onInterval"/> in seconds.</param>
	/// <param name="onInterval">Action to run every interval (e.g., update UI or timer).</param>
	/// <param name="onFinish">Action to run after countdown completes.</param>
	public static CoroutineHandle Start(float duration, float interval, Action<float, int> onInterval, Action onFinish)
	{
		return Start(null, duration, interval, 0, onInterval, onFinish);
	}
	public static CoroutineHandle Start(string key, float duration, float interval, Action<float, int> onInterval, Action onFinish)
	{
		return Start(key, duration, interval, 0, onInterval, onFinish);
	}
	public static CoroutineHandle Start(string key, float duration, float interval, float delay, Action<float, int> onInterval, Action onFinish)
	{
		CoroutineHandle handle = Timing.RunCoroutine(CooldownCoroutine(duration, interval, delay, onInterval, onFinish));
		if (key is not null) NamedCooldowns.Add(key, handle);
		else UnnamedCooldowns.Add(handle);
		return handle;
	}

	public static void Stop(string key)
	{
		if (!NamedCooldowns.TryGetValue(key, out CoroutineHandle handle)) return;
		Timing.KillCoroutines(handle);
		NamedCooldowns.Remove(key);
	}

	public static void StopAll()
	{
		foreach (var handle in NamedCooldowns) Timing.KillCoroutines(handle.Value);
		foreach (var handle in UnnamedCooldowns) Timing.KillCoroutines(handle);
	}

	private static IEnumerator<float> CooldownCoroutine(float duration, float interval, float delay, Action<float, int> onInterval, Action onFinish)
	{
		yield return Timing.WaitForSeconds(delay);
		float remaining = duration;
		int iteration = 0;

		while (remaining > 0f)
		{
			onInterval?.Invoke(remaining, iteration);
			yield return Timing.WaitForSeconds(interval);
			remaining -= interval;
			iteration++;
		}

		onFinish?.Invoke();
	}
}