namespace VEvents.Events.ZombieSurvival;

internal enum State
{
	PreRound,        // Waiting/setup phase before the round starts
	Starting,        // Time when everyone is locked and have the guide messages shown
	SurvivorsReleased,  // Hiders have been released into the map
	ZombiesReleased, // Lights out, zombies are now fully active
	Ended            // Event concluded
}