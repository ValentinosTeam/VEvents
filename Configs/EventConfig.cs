using System.Collections.Generic;
using System.ComponentModel;
using VEvents.Core.Interfaces;

namespace VEvents.Configs;

public class EventConfig
{
	public bool IsEnabled { get; set; } = false;
	public bool IsAuto { get; set; } = true;
	public int Weight { get; set; } = 1;
	public int MinPlayers { get; set; } = 0;
	/// <summary>
	/// Override if you need to change any of the default values of EventConfig.
	/// </summary>
	public virtual void Initialize() { }
}