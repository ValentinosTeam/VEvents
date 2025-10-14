using System;
using System.Collections.Generic;
using MEC;

namespace VEvents.Helpers;

public class CooldownUtils
{
	/// <summary>
	/// Starts a cooldown coroutine that runs an interval action repeatedly during the countdown,
	/// and executes a final action when finished.
	/// </summary>
	/// <param name="duration">Total countdown duration in seconds.</param>
	/// <param name="interval">How often to invoke <paramref name="onInterval"/> in seconds.</param>
	/// <param name="onInterval">Action to run every interval (e.g., update UI or timer).</param>
	/// <param name="onFinish">Action to run after countdown completes.</param>
	public static void Start(float duration, float interval, Action<float, int> onInterval, Action onFinish)
	{
		Timing.RunCoroutine(CooldownCoroutine(duration, interval, onInterval, onFinish));
	}

	private static IEnumerator<float> CooldownCoroutine(float duration, float interval, Action<float, int> onInterval, Action onFinish)
	{
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