using System;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	public class PermissionMeta
	{
		/// <summary>
		/// The revision of the permission
		/// </summary>
		/// <value>The revision.</value>
		public int Revision { get; set; }
		/// <summary>
		/// Date the permission was created
		/// </summary>
		/// <value>The created.</value>
		public DateTime Created { get; set; }
		/// <summary>
		/// The permission version
		/// </summary>
		/// <value>The version.</value>
		public int Version { get; set; }
		/// <summary>
		/// (Optional) Date the permisson was last updated
		/// </summary>
		/// <value>The updated.</value>
		public DateTime Updated { get; set; }

		public static PermissionMeta Parse(JObject m)
		{
			PermissionMeta meta = new PermissionMeta();

			if (m["revision"] != null)
				meta.Revision = m["revision"].Value<int>();

			if (m["created"] != null)
				meta.Created = TypeUtils.UnixEpoch.AddMilliseconds(m["created"].Value<long>());

			if (m["version"] != null)
				meta.Version = m["version"].Value<int>();

			if (m["updated"] != null)
				meta.Updated = TypeUtils.UnixEpoch.AddMilliseconds(m["updated"].Value<long>());

			return meta;
		}
	}
}
