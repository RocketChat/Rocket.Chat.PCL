using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	/// <summary>
	/// Represents a custom emoji.
	/// </summary>
	public class CustomEmoji
	{
		/// <summary>
		/// The emoji id
		/// </summary>
		/// <value>The identifier.</value>
		public string Id { get; set; }
		/// <summary>
		/// The emoji friendly name
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }
		/// <summary>
		/// A collection of alias for the emoji. The alias is used to identify the emoji on text and for fast 
		/// reference from typing - the famous :emoji-alias:. (Each emoji alias is unique per server)
		/// </summary>
		/// <value>The aliases.</value>
		public List<string> Aliases { get; set; }
		/// <summary>
		/// The emoji file extension
		/// </summary>
		/// <value>The extension.</value>
		public string Extension { get; set; }
		/// <summary>
		/// The date when the emoji was updated to the server
		/// </summary>
		/// <value>The last updated.</value>
		public DateTime LastUpdated { get; set; }

		public static CustomEmoji Parse(JObject m)
		{
			CustomEmoji emoji = new CustomEmoji();

			if (m["_id"] != null)
				emoji.Id = (m["_id"] as JValue).Value<string>();

			if (m["name"] != null)
				emoji.Name = (m["name"] as JValue).Value<string>();

			if (m["roles"] != null)
			{
				var roles = m["roles"];
				emoji.Aliases = new List<string>();
				foreach (var obj in roles as JArray)
					emoji.Aliases.Add((obj as JValue).Value<string>());
			}

			if (m["extension"] != null)
				emoji.Extension = (m["extension"] as JValue).Value<string>();

			//	Todo process date

			return emoji;
		}
	}
}
