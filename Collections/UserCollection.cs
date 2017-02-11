using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MeteorPCL;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	public delegate void UserStatusChangedEventArgs(User user);

	public class UserCollection: AbstractCollection<User>, IUserCollection
	{
		public event UserStatusChangedEventArgs UserStatusChanged;

		public UserCollection(IMeteor meteor) : base(meteor, "users") { }

		protected override void OnItemAdded(string id, string collection, JObject obj)
		{
			Update(id, obj);
		}

		protected override void OnItemDeleted(string id, string collection)
		{
			if (_items.ContainsKey(id))
			{
				_items.Remove(id);
			}
		}

		protected override void OnItemUpdated(string id, string collection, JObject obj)
		{
			Update(id, obj);
		}

		private void Update(string id, JObject obj)
		{
			User user;

			if (_items.ContainsKey(id))
			{
				user = _items[id];
			}
			else
			{
				user = new User() { Id = id };
				_items[id] = user;
			}

			User.Update(user, obj);

			if (UserStatusChanged != null)
			{
				UserStatusChanged(user);
			}
		}
	}
}
