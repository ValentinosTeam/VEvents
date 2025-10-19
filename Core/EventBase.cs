using System;
using System.IO;
using System.Text.RegularExpressions;
using LabApi.Features.Console;
using LabApi.Loader;
using VEvents.Configs;
using VEvents.Core.Interfaces;

namespace VEvents.Core;

/// <summary>
/// Basic Event that handles safe starting and stopping and Loads custom configs.
/// </summary>
/// <typeparam name="TConfig"> Create your own config in the event namespace that inherits EventConfig and pass it as a parameter </typeparam>
public abstract class EventBase<TConfig> : IEvent where TConfig : EventConfig, new()
{
	public virtual string Name => GetType().Name;
	public virtual string Description { get; } = "No description provided.";
	public bool IsRunning { get; private set; } = false;
	protected TConfig Config { get; private set; } = new();

	public void Start()
	{
		if (IsRunning)
		{
			Logger.Warn($"{Name} is already running.");
			return;
		}
		IsRunning = true;
		Logger.Info("Starting event: " + Name);
		try
		{
			OnStart();
		} catch (Exception ex)
		{
			Logger.Error($"Failed to start {Name}: {ex}");
			Stop();
			return;
		}
	}
	public void Stop()
	{
		if (!IsRunning)
		{
			Logger.Warn($"{Name} is not running.");
			return;
		}
		IsRunning = false;
		Logger.Info("Stopping event: " + Name);
		try
		{
			OnStop();
		} catch (Exception ex)
		{
			Logger.Error($"Failed to stop {Name}: {ex}");
		}
	}
	public void Validate()
	{
		if (!Regex.IsMatch(Name, "^[a-zA-Z_]+$")) throw new InvalidOperationException($"Invalid event name: {Name}");
	}

	/// <summary>
	/// Override to add custom start conditions.
	/// </summary>
	/// <returns> Returns true if the event has met its conditions to start, false otherwise. </returns>
	public virtual bool CanStartManually(out string response)
	{
		response = null;
		return false;
	}
	public virtual bool CanStartAutomatically(out string response)
	{
		response = null;
		return false;
	}


	public void LoadConfig()
	{
		string fileName = $"event-{Name}-config.yml";
		if (!VEvents.Instance.TryReadConfig(fileName, out TConfig cfg)) // Not using TryLoadConfig to reliably create a new config if it doesn't exist
		{
			Logger.Debug($"[{Name}] Config not found. Creating new one with defaults.");
			cfg = new TConfig();
			cfg.Initialize();	// Changes defaults if defined in Initialize
			bool success = VEvents.Instance.TrySaveConfig(cfg, fileName);
			Logger.Debug($"Creating config was {(success ? "successful" : "unsuccessful")}.");
		}
		Config = cfg;
	}

	protected abstract void OnStart();
	protected abstract void OnStop();
}