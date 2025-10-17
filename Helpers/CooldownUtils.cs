using System;
using System.Collections.Generic;
using MEC;

namespace VEvents.Helpers;

public class CooldownUtils
{
	private static Dictionary<string, CoroutineHandle> _namedCooldowns = new();
	private static List<CoroutineHandle> _unnamedCooldowns = new();

	/// <summary>
	/// Starts a cooldown coroutine that runs an interval action repeatedly during the countdown, and executes a final action when finished.
	/// </summary>
	/// <param name="duration">Total countdown duration in seconds.</param>
	/// <param name="interval">How often to invoke <paramref name="onInterval"/> in seconds.</param>
	/// <param name="onInterval">Action to run every interval (e.g., update UI or timer).</param>
	/// <param name="onFinish">Action to run after countdown completes.</param>
	public static void Start(float duration, float interval, Action<float, int> onInterval, Action onFinish)
	{
		Start(null, duration, interval, 0, onInterval, onFinish);
	}
	public static void Start(string key, float duration, float interval, Action<float, int> onInterval, Action onFinish)
	{
		Start(key, duration, interval, 0, onInterval, onFinish);
	}
	public static void Start(string key, float duration, float interval, float delay, Action<float, int> onInterval, Action onFinish)
	{
		CoroutineHandle handle = Timing.RunCoroutine(CooldownCoroutine(duration, interval, delay, onInterval, onFinish));
		if (key is not null) _namedCooldowns.Add(key, handle);
		else _unnamedCooldowns.Add(handle);
	}

	public static void Stop(string key)
	{
		if (!_namedCooldowns.TryGetValue(key, out CoroutineHandle handle)) return;
		Timing.KillCoroutines(handle);
		_namedCooldowns.Remove(key);
	}

	public static void StopAll()
	{
		foreach (var handle in _namedCooldowns) Timing.KillCoroutines(handle.Value);
		foreach (var handle in _unnamedCooldowns) Timing.KillCoroutines(handle);
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