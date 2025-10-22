using System;
using HarmonyLib;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using VEvents.Configs;
using VEvents.Core;
using VEvents.Extensions;

namespace VEvents;

public class VEvents : Plugin<PluginConfig>
{
	public static VEvents Instance { get; private set; }
	public new PluginConfig Config;
	public VEventManager EventManager { get; private set; }
	private VEventListener EventListener { get; set; }
	private Harmony _harmony;

	public override void Enable()
	{
		Logger.Debug("Starting patching");
		_harmony = new Harmony("gg.valentinos.vevents");
		_harmony.PatchAll();
		Instance = this;
		EventManager = new VEventManager();
		EventListener = new VEventListener();

		CustomHandlersManager.RegisterEventsHandler(EventListener);
	}

	public override void Disable()
	{
		EventManager.StopAllEvents();
		CustomHandlersManager.UnregisterEventsHandler(EventListener);
		_harmony.UnpatchAll(_harmony.Id);
		EventListener = null;
		EventManager = null;
		Instance = null;
	}

	public override void LoadConfigs()
	{
		if (this.TryLoadConfig("config.yml", out Config)) return;
		Logger.Error("Failed to load config. Using default.");
		Config = new PluginConfig();
	}

	public override string Name { get; } = "VEvents";
	public override string Description { get; } = "VEvents, short for Valentinos Events, this plugin is a collection of custom events for the SCP:SL game. Some events are automatic, some are manual. Everything is configurable.";
	public override string Author { get; } = "Alex_Joo";
	public override Version Version { get; } = new Version(1, 0, 1);
	public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);
}
