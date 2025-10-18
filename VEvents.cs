using System;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using VEvents.Configs;
using VEvents.Core;
using VEvents.Events.ZombieSurvival;

namespace VEvents;

public class VEvents : Plugin<PluginConfig>
{
	public static VEvents Instance { get; private set; }
	public new PluginConfig Config { get; private set; }
	public VEventManager EventManager { get; private set; }
	private VEventListener EventListener { get; set; }

	public override void Enable()
	{
		Instance = this;
		EventManager = new VEventManager();
		EventListener = new VEventListener();

		CustomHandlersManager.RegisterEventsHandler(EventListener);
	}

	public override void Disable()
	{
		EventManager.StopAllEvents();
		CustomHandlersManager.UnregisterEventsHandler(EventListener);

		EventListener = null;
		EventManager = null;
		Instance = null;
	}

	public override void LoadConfigs()
	{
		if (!this.TryLoadConfig("config.yml", out PluginConfig Config))
		{
			Logger.Error("Failed to load config. Using default.");
			Config = new PluginConfig();
		}
	}

	public override string Name { get; } = "VEvents";
	public override string Description { get; } = "VEvents, short for Valentinos Events, this plugin is a collection of custom events for the SCP:SL game. Some events are automatic, some are manual. Everything is configurable.";
	public override string Author { get; } = "Alex_Joo";
	public override Version Version { get; } = new Version(0, 0, 0);
	public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);
}
