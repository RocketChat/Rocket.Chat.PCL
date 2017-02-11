using System;
using Newtonsoft.Json;

namespace RocketChatPCL
{
	public class UserLoginRequest
	{
		[JsonProperty("user")]
		public string Username { get; set; }
		[JsonProperty("password")]
		public string Password { get; set; }
	}
}
