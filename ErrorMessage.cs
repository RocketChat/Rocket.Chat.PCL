using System;
using Newtonsoft.Json;

namespace Messenger.RocketChat
{
	/// <summary>
	/// Representation of an error message from the REST API.
	/// </summary>
	public class ErrorMessage
	{
		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("message")]
		public string Text { get; set; }
	}
}
