using System.Collections.Generic;
using System.ComponentModel;
using VEvents.Core.Interfaces;

namespace VEvents.Configs;

public class EventConfig
{
	[Description("==============================Main Settings==============================\n" +
	             "#Whether the event is enabled or not.")]
	public bool IsEnabled { get; set; } = false;
	[Description("Whether the event can be started automatically by the event manager.")]
	public bool IsAuto { get; set; } = false;
	[Description("Whether the event can be started manually by an admin.")]
	public bool IsManual { get; set; } = true;
	[Description("The weight of the event when being selected for automatic events. Higher weight means higher chance of being selected.")]
	public int Weight { get; set; } = 1;
	[Description("The minimum number of players required to start the event.")]
	public int MinPlayers { get; set; } = 0;

	/// <summary>
	/// Override if you need to change any of the default values of EventConfig.
	/// </summary>
	public virtual void Initialize() { }
}