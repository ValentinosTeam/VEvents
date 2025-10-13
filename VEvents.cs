using System;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;
using VEvents.Core;

namespace VEvents;

public class VEvents : Plugin
{
	public static VEvents Instance { get; private set; }
	public static VEventManager EventManager { get; private set; } = new VEventManager();

	public override void Enable()
	{
		Instance = this;
	}

	public override void Disable()
	{
		Instance = null;
	}

	public override string Name { get; } = "VEvents";
	public override string Description { get; } = "VEvents, short for Valentinos Events, this plugin is a collection of custom events for the SCP:SL game. Some events are automatic, some are manual. Everything is configurable.";
	public override string Author { get; } = "Alex_Joo";
	public override Version Version { get; } = new Version(0, 0, 0);
	public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);
}
