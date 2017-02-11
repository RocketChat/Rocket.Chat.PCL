using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	/// <summary>
	/// The permission object describes a permisson
	/// </summary>
	public class Permission
	{
		/// <summary>
		/// The permission’s id
		/// </summary>
		/// <value>The identifier.</value>
		public string Id { get; set; }
		/// <summary>
		/// A collection of roles that this permissions applies to
		/// </summary>
		/// <value>The roles.</value>
		public List<string> Roles { get; set; }
		/// <summary>
		/// (Optional) The last time this permission object was updated in the database
		/// </summary>
		/// <value>The updated at.</value>
		public DateTime UpdatedAt { get; set; }
		/// <summary>
		/// Metadata about the permission
		/// </summary>
		/// <value>The meta.</value>
		public PermissionMeta Meta { get; set; }

		public static Permission Parse(JObject m)
		{
			Permission permission = new Permission();

			if (m["_id"] != null)
				permission.Id = m["_id"].Value<string>();

			permission.Roles = new List<string>();
			if (m["roles"] != null)
				foreach (var role in m["roles"] as JArray)
					permission.Roles.Add(role.Value<string>());

			if (m["_updatedAt"] != null)
				permission.UpdatedAt = TypeUtils.ParseDateTime(m["_updatedAt"] as JObject);

			if (m["meta"] != null)
				permission.Meta = PermissionMeta.Parse(m["meta"] as JObject);

			return permission;
		}

	}
}
