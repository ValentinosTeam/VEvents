using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using LabApi.Loader;
using VEvents.Configs;
using VEvents.Core;
namespace VEvents.Events;

public class TestEventConfig : EventConfig
{
	public string TestString { get; set; } = "Hello, World!";

	public override void Initialize()
	{
		IsEnabled = true;
	}
}
public class TestEvent : EventBase<TestEventConfig>
{
	public override string Name { get; } = "Test Event";
	public override string Description { get; } = "This is a test event for testing purposes.";

	protected override void OnStart()
	{
		Logger.Info("TestEvent started");
		Cassie.Message(Settings.TestString, true, true, true, "Test");
	}

	protected override void OnStop()
	{
		Logger.Info("TestEvent stopped");
	}
}