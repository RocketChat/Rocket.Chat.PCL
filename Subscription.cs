using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	public class Subscription
	{
		public RoomType Type { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime LastSeenDate { get; set; }
		public string Name { get; set; }
		public string RoomId { get; set; }
		public User CreatedBy { get; set; }
		public bool IsOpen { get; set; }
		public bool IsAlert { get; set; }
		public int UnreadMessages { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string Id { get; set; }
		public List<string> Roles { get; set; }

		public static Subscription Parse(JObject m)
		{
			var subscription = new Subscription();

			if (m["t"] != null) {
				switch (m["t"].ToString()) {
				case "d":
					subscription.Type = RoomType.DirectMessage;
					break;
				case "p":
					subscription.Type = RoomType.PrivateGroup;
					break;
				case "c":
					subscription.Type = RoomType.PublicChannel;
					break;
				}
			}

			if (m["ts"] != null)
				subscription.CreatedDate = TypeUtils.ParseDateTime(m["ts"] as JObject);

			if (m["ls"] != null)
				subscription.LastSeenDate = TypeUtils.ParseDateTime(m["ls"] as JObject);

			if (m["name"] != null)
				subscription.Name = m["name"].ToString();

			if (m["rid"] != null)
				subscription.RoomId = m["rid"].ToString();

			if (m["u"] != null)
				subscription.CreatedBy = User.Parse(m["u"] as JObject);

			if (m["open"] != null)
				subscription.IsAlert = (m["open"] as JValue).Value<bool>();

			if (m["alert"] != null)
				subscription.IsOpen = (m["alert"] as JValue).Value<bool>();
			
			subscription.Roles = new List<string>();
			if (m["roles"] != null)
				foreach (var role in m["roles"] as JArray)
					subscription.Roles.Add(role.Value<string>());

			if (m["unread"] != null)
				subscription.UnreadMessages = (m["unread"] as JValue).Value<Int32>();

			if (m["_id"] != null)
				subscription.Id = (m["_id"] as JValue).Value<string>();

			return subscription;
		}
	}
}
