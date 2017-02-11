using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	public class RoomRole
	{
		/// <summary>
		/// the id of this object
		/// </summary>
		/// <value>The identifier.</value>
		public string Id { get; set; }
		/// <summary>
		/// A simple user object with the user id and username
		/// </summary>
		/// <value>The user.</value>
		public User User { get; set; }
		/// <summary>
		/// The collection of roles of the user in the room
		/// </summary>
		/// <value>The roles.</value>
		public List<string> Roles { get; set; }
		/// <summary>
		/// The room id this user and role belongs to
		/// </summary>
		/// <value>The identifier.</value>
		public string RoomId { get; set; }

		public static RoomRole Parse(JObject m)
		{
			RoomRole role = new RoomRole();

			if (m["_id"] != null) 
				role.Id = m["_id"].Value<string>();

			if (m["u"] != null)
				role.User = User.Parse(m["u"] as JObject);

			role.Roles = new List<string>();
			if (m["roles"] != null)
				foreach (var r in m["roles"] as JArray)
					role.Roles.Add(r.Value<string>());

			if (m["rid"] != null)
				role.RoomId = m["rid"].Value<string>();

			return role;
		}
	}
}
