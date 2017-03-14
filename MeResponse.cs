using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RocketChatPCL
{
	public class MeResponse
	{
		[JsonProperty("_id")]
		public string Id { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("emails")]
		public List<MeEmailAddress> Emails { get; set; }
		[JsonProperty("status")]
		public string Status { get; set; }
		[JsonProperty("statusConnection")]
		public string StatusConnection { get; set; }
		[JsonProperty("username")]
		public string Username { get; set; }
		[JsonProperty("utcOffset")]
		public int UtcOffset { get; set; }
		[JsonProperty("active")]
		public bool Active { get; set; }
		[JsonProperty("success")]
		public bool Success { get; set; }
	}
}
