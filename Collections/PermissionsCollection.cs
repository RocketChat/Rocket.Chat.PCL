using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeteorPCL;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	public class PermissionsCollection : AbstractCollection<Permission>, IPermissionsCollection
	{
		private Dictionary<string, HashSet<Permission>> _permissionsByRole;

		public PermissionsCollection(IMeteor meteor): base(meteor)
		{
			_permissionsByRole = new Dictionary<string, HashSet<Permission>>();
		}

		public Task Initialize(string userId, DateTime since)
		{
			var task = new Task(() =>
			{
				var permissions = GetPermissions();
				permissions.Wait();

				foreach (var permission in permissions.Result)
				{
					_items.Add(permission.Id, permission);

					foreach (var role in permission.Roles)
					{
						if (!_permissionsByRole.ContainsKey(role))
						{
							_permissionsByRole[role] = new HashSet<Permission>();
						}
						_permissionsByRole[role].Add(permission);
					}
				}
			});

			task.Start();

			return task;
		}

		/// <summary>
		/// Use this call to get a collection with all the permissions of the server. Each permission will have the roles it applies to.
		/// You may use this information to change your UI according to the permissons a user has (hidding what he can’t do for example).
		/// </summary>
		/// <returns>The permissions.</returns>
		public Task<List<Permission>> GetPermissions()
		{
			return _meteor.CallWithResult("permissions/get", new object[] { })
						   .ContinueWith((arg) =>
			{
				List<Permission> permissions = new List<Permission>();
				var res = arg.Result;
				if (res == null || res["result"] == null)
				{
					return permissions;
				}

				foreach (var item in res["result"] as JArray)
				{
					permissions.Add(Permission.Parse(item as JObject));
				}

				return permissions;
			});
		}

		protected override void OnItemAdded(string id, string collection, JObject obj)
		{
			// Do nothing.
		}

		protected override void OnItemUpdated(string id, string collection, JObject obj)
		{
			//	Do nothing.
		}

		protected override void OnItemDeleted(string id, string collection)
		{
			//	Do nothing.
		}
	}
}
