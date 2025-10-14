using System.ComponentModel;

namespace VEvents.Configs;

public class PluginConfig
{
	[Description("If true, any enabled automatic events will trigger automatically based on their conditions.")]
	public bool AllowAutoEvents { get; set; } = true;
	public int RoundStartNoEventWeight { get; set; } = 50;
	public int MidRoundNoEventWeight { get; set; } = 25;
}