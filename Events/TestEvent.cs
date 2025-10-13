using LabApi.Features.Console;
using VEvents.Core;

namespace VEvents.Events;

public class TestEvent : EventBase
{
	public override string Name { get; } = "Test Event";
	public override string Description { get; } = "This is a test event for testing purposes.";

	protected override void OnStart()
	{
		Logger.Info("TestEvent started");
	}

	protected override void OnStop()
	{
		Logger.Info("TestEvent stopped");
	}
}