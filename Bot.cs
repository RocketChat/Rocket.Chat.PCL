using System;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	/// <summary>
	/// Represents a bot
	/// </summary>
	public class Bot
	{
		/// <summary>
		/// The unique ID of the bot.
		/// </summary>
		/// <value>The identifier.</value>
		public string Id { get; set; }

		/// <summary>
		/// Parse the specified jSON object into a bot.
		/// </summary>
		/// <returns>The bot</returns>
		/// <param name="m">The json object to parse</param>
		public static Bot Parse(JObject m)
		{
			Bot bot = new Bot();

			if (m["i"] != null)
				bot.Id = m["i"].Value<string>();

			return bot;
		}
	}
}
