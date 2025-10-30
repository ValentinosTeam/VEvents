using System.Collections.Concurrent;
using System.Collections.Generic;
using GameCore;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using RueI.API;
using RueI.API.Elements;

namespace VEvents.Helpers;

public static class RueiUtils
{
	private static readonly ConcurrentDictionary<string, Tag> _tags = new();

	/// <summary>
	/// Sends a simple RueI hint to the specified player.
	/// </summary>
	/// <param name="player">Target player.</param>
	/// <param name="message">Hint message.</param>
	/// <param name="duration">How long the hint should last (seconds).</param>
	/// <param name="id">The id of the tag, specify if you plan to replace hints</param>
	/// <param name="position"> The position of the hint on the players hud </param>
	public static void SendHint(Player player, string message, float duration = 3f, string id = null, int position = 800)
	{

		if (player == null) return;
		RueDisplay display = RueDisplay.Get(player);
		Tag tag = null;
		if (id == null) tag = new Tag();
		else
		{
			if (!_tags.TryGetValue(id, out tag))
			{
				tag = new Tag(id);
				_tags[id] = tag;
			}
		}
		var element = new BasicElement(position, message);
		display.Show(tag, element, duration);
	}
}