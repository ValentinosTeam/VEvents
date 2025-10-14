namespace VEvents.Core.Interfaces;

public interface IEventConfig
{
	public bool IsEnabled { get; set; }

	public void Validate();
	public void Initialize();
}