using System.Collections.Generic;
using System.ComponentModel;

namespace VEvents.Events.AutoFf;

public class Config : Configs.EventConfig
{
	public override void Initialize()
	{
		IsEnabled = true;
		IsAuto = true;
		IsManual = false;
		MinPlayers = 0;
	}

	[Description("If any of the following events are running, this event wont change the friendly fire state.")]
	public List<string> CantRunWith { get; set; } =
	[
		"ZombieSurvival"
	];
}