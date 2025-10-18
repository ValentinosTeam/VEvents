using System.Collections.Generic;
using System.ComponentModel;

namespace VEvents.Events.ZombieSurvival;

public class Config : Configs.EventConfig
{
	public override void Initialize()
	{
		IsEnabled = true;
		IsAuto = false;
		IsManual = true;
		MinPlayers = 6;
	}

	[Description("==============================Zombie Survival Settings==============================\n"+
	             "# The ratio of zombies to total players.")]
	public float ZombieRatio { get; set; } = 1f/6f;

	[Description("Message shown to all players in the Pre Round state.")]
	public string EventStartingMessage { get; set; } = "Zombie Survival is starting!";

	[Description("Messages shown to players when they are assigned as zombies or survivors. Has to be at least 1 message and the same amount.")]
	public List<string> ZombieGuideMessages { get; set; } =
	[
		"You are a Zombie! Find and kill the Survivors!",
		"You can see in the dark!",
		"You will be released soon!",
	];
	public List<string> SurvivorGuideMessages { get; set; } =
	[
		"You are a Survivor! Avoid the Zombies!",
		"Use weapons to fight back and get loot!",
		"Lights are off any minute now!",
	];

	[Description("The amount of time in seconds to show each of the guide messages. The event wont start until all messages have been shown.")]
	public int GuideMessageInterval { get; set; } = 10;

	[Description("The delay in seconds before zombies are released to hunt survivors. Starts after guide messages have been shown.")]
	public int ZombieReleaseDelay { get; set; } = 120; // 120 = 2 minutes. Time before zombies are released
	[Description("Message shown to all players indicating how much time is left until zombies are released. {0} is replaced with the time in seconds.")]
	public string TimeUntilZombiesReleasedMessage { get; set; } = "Zombies will be released in {0} seconds!";

	[Description("The duration in seconds of how much time zombies have to find and convert all survivors before the event ends.")]
	public int EventDuration { get; set; } = 900; // 900 s is 15 minutes
	[Description("Message shown to all players indicating how much time is left until the event ends. {0} is replaced with the time in seconds.")]
	public string TimeUntilEventEndsMessage { get; set; } = "Event ends in {0} seconds!";

	[Description("The duration in seconds to show the end message before ending the event.")]
	public int EndEventDuration { get; set; } = 30;
	[Description("If the survivors win, event ending message will show how many survivors {0} are remaining and their names {1}.")]
	public string SurvivorsWinMessage { get; set; } = "Survivors win! {0} survivor(s) remaining: {1}.";
	[Description("If the zombies win.")]
	public string ZombiesWinMessage { get; set; } = "Zombies win! All survivors have been converted.";

	[Description("Items and their quantities that zombies will randomly drop upon death. Repeat entries to increase their chances of being selected.")]
	public List<Dictionary<ItemType, int>> ZombieDrops { get; set; } =
	[
		new() { { ItemType.Adrenaline, 1 } },
		new() { { ItemType.Ammo9x19, 2 } },
		new() { { ItemType.Ammo9x19, 3 } },
		new() { { ItemType.Medkit, 1 } }
	];

	[Description("Sub events that can occur during the event to add more chaos. Interval is random between min and max.")]
	public float SubEventMinInterval { get; set; } = 30f;
	public float SubEventMaxInterval { get; set; } = 90f;

	[Description("Weights determine the chance of each event occurring.")]
	public Dictionary<SubEvent, int> SubEventWeights { get; set; } = new()
	{
		{SubEvent.None, 10},
		{SubEvent.Cassie, 5},
		{SubEvent.Flicker, 4},
		{SubEvent.Amnesia, 3}
	};
}