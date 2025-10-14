using System.Collections.Generic;
using VEvents.Configs;

namespace VEvents.Events;

public class ZombieSurvivalConfig : EventConfig
{
	public float SeekerRatio { get; set; } = 1f/6f; // 1/6th of players are seekers
	public float ZombieReleaseDelay { get; set; } = 120f; // Time before zombies are released

	public List<string> SeekerGuideMessages { get; set; } =
	[
		"You are a seeker! Find and kill the hiders!",
		"You can see in the dark!",
		"You will be released soon!",
	];
	public List<string> HiderGuideMessages { get; set; } =
	[
		"You are a hider! Avoid the zombies!",
		"Use weapons to fight back!",
		"Lights are off any minute now!",
	];
	public int GuideMessageInterval { get; set; } = 10;

}