using System;
using Newtonsoft.Json;

namespace RocketChatPCL
{
	public class MeEmailAddress
	{
		[JsonProperty("address")]
		public string Address { get; set; }
		[JsonProperty("verified")]
		public bool Verified { get; set; }
	}
}
