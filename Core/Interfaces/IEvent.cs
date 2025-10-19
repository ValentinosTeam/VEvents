using VEvents.Configs;

namespace VEvents.Core.Interfaces;

public interface IEvent
{
	public string Name { get; }
	public string Description { get; }
	public bool IsRunning { get; }
	public void Start();
	public void Stop();
	public void LoadConfig();
	public void Validate();
	public bool CanStartManually(out string response);
	public bool CanStartAutomatically(out string response);
}