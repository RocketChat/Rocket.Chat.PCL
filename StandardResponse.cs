using System;
using Newtonsoft.Json;

namespace RocketChatPCL
{
	public class StandardResponse<T>
	{
		[JsonProperty("status")]
		public string Status { get; set; }
		[JsonProperty("data")]
		public T Data { get; set; }
	}
}
