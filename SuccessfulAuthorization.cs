using System;
using Newtonsoft.Json;

namespace RocketChatPCL
{
	public class SuccessfulAuthorization
	{
		[JsonProperty("userId")]
		public string UserId { get; set; }
		[JsonProperty("authToken")]
		public string AuthToken { get; set; }
	}
}
