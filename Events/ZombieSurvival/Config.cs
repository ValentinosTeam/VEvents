using System.Collections.Generic;
using System.ComponentModel;

namespace VEvents.Events.ZombieSurvival;

public class CrowdingEffectConfig
{
	public int Threshold { get; set; }
	public string Hint { get; set; }
	public string Effect { get; set; }
	public byte Intensity { get; set; }
}

public class Config : Configs.EventConfig
{
	public override void Initialize()
	{
		IsEnabled = true;
		IsAuto = false;
		IsManual = true;
		MinPlayers = 6;
	}

	[Description("==============================Zombie Survival General Settings==============================\n" +
	             "# The ratio of zombies to total players.")]
	public float ZombieRatio { get; set; } = 1f / 6f;

	[Description("The delay in seconds before zombies are released to hunt survivors. Starts after guide messages have been shown.")]
	public int ZombieReleaseDelay { get; set; } = 120;

	[Description("The duration in seconds of how much time zombies have to find and convert all survivors before the event ends.")]
	public int EventDuration { get; set; } = 600;

	[Description("The duration in seconds to allow the winners to stay in the celebrating tower.")]
	public int EndEventDuration { get; set; } = 30;

	[Description("The health amount of the first wave of the zombies.")]
	public int FirstZombiesHealth { get; set; } = 600;
	[Description("The health amount of any respawning zombies.")]
	public int ZombiesHealth { get; set; } = 300;
	[Description("The health amount of survivors.")]
	public int SurvivorHealth { get; set; } = 160;

	[Description("Items and their quantities that zombies will randomly drop upon death. Repeat entries to increase their chances of being selected.")]
	public List<Dictionary<ItemType, int>> ZombieDrops { get; set; } =
	[
		new() { { ItemType.GunCrossvec, 2 } },
		new() { { ItemType.Adrenaline, 2 } },
		new() { { ItemType.Painkillers, 4 } },
		new() { { ItemType.Ammo9x19, 3 } },
		new() { { ItemType.Ammo9x19, 3 } },
		new() { { ItemType.Ammo9x19, 3 } },
		new() { { ItemType.Ammo9x19, 3 } },
		new() { { ItemType.Ammo556x45, 4 } },
		new() { { ItemType.Medkit, 2 } },
		new() { { ItemType.Medkit, 2 } },
		new() { { ItemType.GrenadeFlash, 1 } },
		new() { { ItemType.GrenadeHE, 1 } },
		new() { { ItemType.GunShotgun, 1 } },
		new() { { ItemType.GunLogicer, 1 } }
	];

	[Description("Item list the survivors will spawn with at the start of the event.")]
	public List<Dictionary<ItemType, int>> SurvivorSpawnItems { get; set; } =
	[
		new() { { ItemType.Ammo9x19, 4 } },
		new() { { ItemType.GunCOM18, 1 } },
		new() { { ItemType.Lantern, 1 } },
		new() { { ItemType.KeycardChaosInsurgency, 1 } }
	];

	[Description("==============================Zombie Survival Messages==============================\n" +
	             "# Message shown to all players in the Pre Round state.")]
	public string EventStartingMessage { get; set; } = "<color=#C21010>Z</color><color=#BB1210>O</color><color=#B41410>M</color><color=#AD1610>B</color><color=#A61810>I</color><color=#9F1A10>E</color> <color=#911E10>S</color><color=#8A2010>U</color><color=#832210>R</color><color=#7C2410>V</color><color=#752610>I</color><color=#6E2810>V</color><color=#672A10>A</color><color=#602C10>L</color> <color=#523010>I</color><color=#4B3210>S</color> <color=#3D3610>S</color><color=#363810>T</color><color=#2F3A10>A</color><color=#283C10>R</color><color=#213E10>T</color><color=#1A4010>I</color><color=#134210>N</color><color=#0C4410>G</color>";

	[Description("Messages shown to players when they are assigned as zombies or survivors. Has to be at least 1 message and the same amount.")]
	public List<string> ZombieGuideMessages { get; set; } =
	[
		"<color=#FF0000><u>You are a Zombie Find and kill all humans \ud83d\udc80</u></color> ",
		"<color=#FF0000><u>All facility lights will shut down but you can see in the dark</u></color>",
		"<color=#FF0000><u>You will be released soon</u></color>",
	];

	public List<string> SurvivorGuideMessages { get; set; } =
	[
		"<color=#FF0000><u>You are a Human survive for as long as possible</u></color>",
		"<color=#FF0000><u>Find weapons to protect yourself kill zombies for gear and ammunition</u></color>",
		"<color=#FF0000><u>All facility lights will soon shut down</u></color>",
	];

	[Description("The amount of time in seconds to show each of the guide messages. The event wont start until all messages have been shown.")]
	public int GuideMessageInterval { get; set; } = 10;

	[Description("Message shown to all players indicating how much time is left until zombies are released. {0} is replaced with the time in seconds.")]
	public string TimeUntilZombiesReleasedMessage { get; set; } = "<color=#FF0000>Zombie outbreak in</color> <b><color=#0f4e06>{0}</color></b>";

	[Description("Message shown to all players indicating how much time is left until the event ends. {0} is replaced with the time in seconds.")]
	public string TimeUntilEventEndsMessage { get; set; } = "<color=#FF0000>{0}</color>";

	[Description("If the survivors win, event ending message will show how many survivors {0} are remaining and their names {1}.")]
	public string SurvivorsWinMessage { get; set; } = "<color=#0f3f65>The Humans survived! Still alive: {1}.</color>";

	[Description("If the zombies win.")]
	public string ZombiesWinMessage { get; set; } = "<color=#FF0000>ALL WAS CONSUMED BY THE ZOMBIE HORDE</color>";

	[Description("==============================Zombie Survival Subevents==============================\n" +
				"# Sub events that can occur during the event to add more chaos. Interval is random between min and max.")]
	public float SubEventMinInterval { get; set; } = 30f;
	public float SubEventMaxInterval { get; set; } = 60f;

	[Description("Weights determine the chance of each event occurring.")]
	public Dictionary<SubEvent, int> SubEventWeights { get; set; } = new()
	{
		{SubEvent.None, 0},
		{SubEvent.Glitch, 2},
		{SubEvent.Amnesia, 2},
		{SubEvent.BackupPower, 1}
	};

	[Description("The range of how long to give the survivors the amnesia effect.")]
	public float AmnesiaDurationMin { get; set; } = 10f;
	public float AmnesiaDurationMax { get; set; } = 30f;

	[Description("==============================Zombie Survival Crowding Debuffs==============================\n" +
	             "# Crowding effects that apply debuffs to survivors based on how many survivors are in the same room to counteract camping together.")]
	public List<CrowdingEffectConfig> CrowdingEffects { get; set; } = new List<CrowdingEffectConfig>
	{
		new CrowdingEffectConfig
		{
			Threshold = 4,
			Hint = "You're in a group, you feel paranoid and out of breath.",
			Effect = "Exhausted",
			Intensity = 1
		},

		new CrowdingEffectConfig
		{
			Threshold = 5,
			Hint = "The amount of people here is blurring your vision, it's hard to focus.",
			Effect = "Blindness",
			Intensity = 60
		},

		new CrowdingEffectConfig
		{
			Threshold = 5,
			Hint = "The amount of people here is blurring your vision, it's hard to focus.",
			Effect = "Deafened",
			Intensity = 1
		},

		new CrowdingEffectConfig
		{
			Threshold = 6,
			Hint = "<color=#d0971a>This group is getting too big, you have to split up!</color>",
			Effect = "Concussed",
			Intensity = 1
		},

		new CrowdingEffectConfig
		{
			Threshold = 7,
			Hint = "<color=#ff0000>There's too many around, it's suffocating! You have to get out of this crowd!!</color>",
			Effect = "Asphyxiated",
			Intensity = 1
		}
	};
	[Description("The position in the player's HUD where crowding effect hints will appear.")]
	public int CrowdingEffectHintPosition { get; set; } = 800;
}