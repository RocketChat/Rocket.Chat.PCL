using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	/// <summary>
	/// Represents a URL attached to a message.
	/// </summary>
	public class AttachedUrl
	{
		/// <summary>
		/// The URL itself (just as it appears on the message)
		/// </summary>
		/// <value>The URL.</value>
		public string Url { get; set; }
		/// <summary>
		/// URL metadata (varies accord to the URL)
		/// </summary>
		/// <value>The meta.</value>
		public Dictionary<string, string> Meta { get; set; }
		/// <summary>
		/// Some HTTP headers (varies accord to the URL)
		/// </summary>
		/// <value>The headers.</value>
		public Dictionary<string, string> Headers { get; set; }
		/// <summary>
		/// The parsed URL broken into its parts
		/// </summary>
		/// <value>The parsed URL.</value>
		public Dictionary<string, string> ParsedUrl { get; set; }

		/// <summary>
		/// Parse the specified JSON obejct into an AttachedUrl
		/// </summary>
		/// <returns>The parse.</returns>
		/// <param name="m">The JSON object</param>
		public static AttachedUrl Parse(JObject m)
		{
			AttachedUrl url = new AttachedUrl();

			if (m == null)
				return url;

			if (m["url"] != null && m["url"] is JValue)
				url.Url = m["url"].Value<string>();

			url.Meta = new Dictionary<string, string>();
			if (m["meta"] != null && m["meta"] is JObject)
				foreach (var key in (m["meta"] as JObject))
					url.Meta.Add(key.Key, key.Value.Value<string>());
				
			url.Headers = new Dictionary<string, string>();
			if (m["headers"] != null && m["headers"] is JObject)
				foreach (var key in (m["headers"] as JObject))
					url.Headers.Add(key.Key, key.Value.Value<string>());

			url.ParsedUrl = new Dictionary<string, string>();
			if (m["parsedUrl"] != null && m["parsedUrl"] is JObject)
				FlattenJObject(url.ParsedUrl, "", m["parsedUrl"] as JObject);

			return url;
		}

		private static void FlattenJObject(Dictionary<string, string> dictionary, string prefix, JObject o)
		{
			foreach (var key in o)
			{
				if (key.Value is JValue)
					dictionary.Add(key.Key, key.Value.Value<string>());
				else if (key.Value is JObject)
					FlattenJObject(dictionary, (prefix != "" ? prefix + "." : "") + key.Key, key.Value as JObject);
			}
		}
	}
}
