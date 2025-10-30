using System.Collections.Generic;
using System.ComponentModel;

namespace VEvents.Events.Test;

public class Config : Configs.EventConfig
{
	public override void Initialize()
	{
		IsEnabled = true;
		IsAuto = false;
		IsManual = true;
		MinPlayers = 0;
	}

}