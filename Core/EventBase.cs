using System;
using LabApi.Features.Console;
using VEvents.Core.Interfaces;

namespace VEvents.Core;

public abstract class EventBase : IEvent
{
	/// <summary>
	/// Event name, no spaces. Has to be unique.
	/// </summary>
	public abstract string Name { get; }
	/// <summary>
	/// Description of the event. Can be used as a help text. Optional, but recommended.
	/// </summary>
	public virtual string DisplayName => Name.Replace("_", " ");
	public virtual string Description { get; } = "No description provided.";
	/// <summary>
	/// Is the event currently running?
	/// </summary>
	public bool IsRunning { get; private set; }

	/// <summary>
	/// Starts the event. Calls OnStart() and sets IsRunning to true.
	/// </summary>
	public void Start()
	{
		if (IsRunning)
		{
			Logger.Warn($"{Name} is already running!");
			return;
		}
		IsRunning = true;
		Logger.Info($"[VEvents] Started event: {Name}");
		try
		{
			OnStart();
		}
		catch (Exception ex)
		{
			Logger.Error($"[VEvents] Exception while starting {Name}: {ex}");
			Stop();
		}
	}
	/// <summary>
	/// Stops the event. Calls OnStop() and sets IsRunning to false.
	/// </summary>
	public void Stop()
	{
		if (!IsRunning)
		{
			Logger.Warn($"{Name} is not running!");
			return;
		}
		IsRunning = false;
		Logger.Info($"[VEvents] Stopped event: {Name}");
		try
		{
			OnStop();
		}
		catch (Exception ex)
		{
			Logger.Error($"[VEvents] Exception while stopping {Name}: {ex}");
		}
	}

	/// <summary>
	/// Called when the event is started. Override this method to implement custom start logic.
	/// </summary>
	protected abstract void OnStart();
	/// <summary>
	/// Called when the event is stopped. Override this method to implement custom stop logic.
	/// </summary>
	protected abstract void OnStop();
}