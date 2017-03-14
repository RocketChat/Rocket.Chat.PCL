using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	public class User
	{
		public string Id { get; set; }
		public string Username { get; set; }
		public List<string> Roles { get; set; }
		public int UtcOffset { get; set; }
		public string Status { get; set; }
		public string Name { get; set; }

		public static User Parse(JObject m)
		{
			User user = new User();

			return Update(user, m);

		}

		public static User Update(User user, JObject m)
		{
			if (m["_id"] != null)
				user.Id = (m["_id"] as JValue).Value<string>();

			if (m["username"] != null)
				user.Username = (m["username"] as JValue).Value<string>();

			if (m["roles"] != null)
			{
				user.Roles = new List<string>();
				foreach (var obj in m["roles"] as JArray)
					user.Roles.Add((obj as JValue).Value<string>());
			}

			if (m["utcOffset"] != null)
				user.UtcOffset = m["utcOffset"].Value<int>();

			if (m["status"] != null)
				user.Status = m["status"].Value<string>();

			if (m["name"] != null)
				user.Name = m["name"].Value<string>();
			
			return user;
		}
	}
}
