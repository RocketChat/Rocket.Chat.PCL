using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MeteorPCL;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	public class RoomCollection: AbstractCollection<Room>, IRoomCollection
	{
		public RoomCollection(IMeteor meteor): base(meteor, "userChannels", "stream-notify-room", "stream-notify-user", "stream-room-messages")
		{
		}

		public event UserTypingEventArgs UserStartedTyping;
		public event UserTypingEventArgs UserStoppedTyping;
		public event RoomMessageReceivedEventArgs MessageReceived;

		public Task Initialize(string userId, DateTime since)
		{
			var task = new Task(() =>
			{
				var rooms = GetRooms(since);
				var subrs = GetSubscriptions(since);
				_meteor.Subscribe("stream-notify-user", new object[] { userId + "/rooms-changed" });
				_meteor.Subscribe("userChannels", new object[] { userId } );
				rooms.Wait();
				subrs.Wait();

				foreach (var room in rooms.Result.Added)
					UpdateRoom(room);

				foreach (var room in rooms.Result.Updated)
					UpdateRoom(room);

				foreach (var room in rooms.Result.Removed)
				{
					if (_items.ContainsKey(room.Id))
						_items.Remove(room.Id);
				}

				foreach (var room in subrs.Result.Added)
					UpdateRoom(room);

				foreach (var room in subrs.Result.Updated)
					UpdateRoom(room);

				foreach (var room in subrs.Result.Removed)
				{
					if (_items.ContainsKey(room.RoomId))
						_items.Remove(room.RoomId);
				}

			});

			task.Start();

			return task;
		}

		private void UpdateRoom(Room room)
		{
			if (_items.ContainsKey(room.Id))
				_items[room.Id] = Room.Update(_items[room.Id], room);
			else
				_items.Add(room.Id, room);
		}

		private void UpdateRoom(Subscription sub)
		{
			if (_items.ContainsKey(sub.RoomId))
				_items[sub.RoomId] = Room.Update(_items[sub.RoomId], sub);
			else
				_items[sub.RoomId] = Room.Update(new Room(_meteor) { Id = sub.RoomId }, sub);
		}

		/// <summary>
		/// This is the method call used to get all the rooms a user belongs to. 
		/// It accepts a timestamp with the latest client update time in order to just send what changed since last 
		/// call. If it’s the first time calling, just send a 0 as date.
		/// </summary>
		/// <returns>The rooms.</returns>
		/// <param name="since">date with the latest client update time in order to just send what changed since last 
		/// call. If it’s the first time calling, just send a Epoch as date.</param>
		public Task<CollectionDiff<Room>> GetRooms(DateTime since)
		{
			return _meteor.CallWithResult("rooms/get", new object[] { new Dictionary<string, object>() { { "$date", TypeUtils.DateTimeToTimestamp(since) } } })
						   .ContinueWith((arg) =>
		   {
			   var res = arg.Result;
			   return ProcessRooms(res);
		   });
		}

		/// <summary>
		/// Returns a collection diff an user’s subscription collection. If a date is passed the result will only contains changes to the subscriptions
		/// </summary>
		/// <returns>The subscriptions.</returns>
		/// <param name="since">Date since the last update</param>
		public Task<CollectionDiff<Subscription>> GetSubscriptions(DateTime since)
		{
			return _meteor.CallWithResult("subscriptions/get", new object[] { new Dictionary<string, object>() { { "$date", TypeUtils.DateTimeToTimestamp(since) } } })
						   .ContinueWith((arg) =>
		   {
			   var res = arg.Result;
			   return ProcessSubscriptions(res);
		   });
		}
		/// <summary>
		/// Create a new public channel.
		/// </summary>
		/// <returns>The channel.</returns>
		/// <param name="name">The name of the channel to create.</param>
		/// <param name="participants">List of usernames of participants.</param>
		/// <param name="readOnly">If set to <c>true</c> read only.</param>
		public Task<Room> CreateChannel(string name, List<string> participants, bool readOnly)
		{
			return _meteor.CallWithResult("createChannel", new object[] { name, participants, readOnly })
						   .ContinueWith((arg) =>
			{
				var res = arg.Result;
				var room = new Room(_meteor);

				room.Name = name;
				room.ReadOnly = readOnly;
				room.Type = RoomType.PublicChannel;

				if (res["result"] != null && res["result"] is JArray)
				{
					var arr = res["result"] as JArray;
					var obj = arr[0] as JObject;

					if (obj["rid"] != null)
					{
						room.Id = obj["rid"].Value<string>();
					}

					return room;
				}

				return room;
			});
		}
		/// <summary>
		/// Create a new private group.
		/// </summary>
		/// <returns>The channel.</returns>
		/// <param name="name">The name of the channel to create.</param>
		/// <param name="participants">List of usernames of participants.</param>
		/// <param name="readOnly">If set to <c>true</c> read only.</param>
		public Task<Room> CreatePrivateGroup(string name, List<string> participants, bool readOnly)
		{
			return _meteor.CallWithResult("createPrivateGroup", new object[] { name, participants, readOnly })
						   .ContinueWith((arg) =>
			{
				var res = arg.Result;
				var room = new Room(_meteor);

				room.Name = name;
				room.ReadOnly = readOnly;
				room.Type = RoomType.PrivateGroup;

				if (res["result"] != null && res["result"] is JArray)
				{
					var arr = res["result"] as JArray;
					var obj = arr[0] as JObject;

					if (obj["rid"] != null)
					{
						room.Id = obj["rid"].Value<string>();
					}

					return room;
				}

				return room;
			});
		}

		private CollectionDiff<Room> ProcessRooms(JObject obj)
		{
			CollectionDiff<Room> rooms = new CollectionDiff<Room>();
			var result = obj["result"];
			if (result == null)
			{
				return rooms;
			}

			var updates = result["update"] != null ? (result as JObject)["update"] as JArray : null;
			var additions = result["add"] != null ? (result as JObject)["add"] as JArray : null;
			var removes = result["remove"] != null ? (result as JObject)["remove"] as JArray : null;

			if (additions != null)
			{
				foreach (var channelTok in additions)
				{
					rooms.Added.Add(Room.Parse(_meteor, channelTok as JObject));
				}
			}

			if (updates != null)
			{
				foreach (var channelTok in updates)
				{
					rooms.Updated.Add(Room.Parse(_meteor, channelTok as JObject));
				}
			}

			if (removes != null)
			{
				foreach (var channelTok in removes)
				{
					rooms.Removed.Add(Room.Parse(_meteor, channelTok as JObject));
				}
			}

			return rooms;
		}

		private CollectionDiff<Subscription> ProcessSubscriptions(JObject obj)
		{
			CollectionDiff<Subscription> subscriptions = new CollectionDiff<Subscription>();
			var result = obj["result"];
			if (result == null)
			{
				return subscriptions;
			}

			var updates = result["update"] != null ? (result as JObject)["update"] as JArray : null;
			var additions = result["add"] != null ? (result as JObject)["add"] as JArray : null;
			var removes = result["remove"] != null ? (result as JObject)["remove"] as JArray : null;

			if (additions != null)
			{
				foreach (var channelTok in additions)
				{
					subscriptions.Added.Add(Subscription.Parse(channelTok as JObject));
				}
			}

			if (updates != null)
			{
				foreach (var channelTok in updates)
				{
					subscriptions.Updated.Add(Subscription.Parse(channelTok as JObject));
				}
			}

			if (removes != null)
			{
				foreach (var channelTok in removes)
				{
					subscriptions.Removed.Add(Subscription.Parse(channelTok as JObject));
				}
			}

			return subscriptions;
		}

		protected override void OnItemAdded(string id, string collection, JObject obj)
		{
			//	Do nothing.
			Debug.WriteLine("Added {0} to {1}: {2}", id, collection, obj);
		}

		protected override void OnItemUpdated(string id, string collection, JObject obj)
		{
			if ("stream-notify-room".Equals(collection) &&
				obj["eventName"] != null && obj["eventName"].Value<string>().EndsWith("/typing"))
			{
				var eventName = obj["eventName"].Value<string>();
				var eventArgs = obj["args"] as JArray;
				if (eventArgs.Count == 2)
				{
					var room = eventName.Substring(0, eventName.IndexOf("/typing"));
					var username = eventArgs[0].Value<string>();
					var isTyping = eventArgs[1].Value<bool>();

					if (isTyping && UserStartedTyping != null)
						UserStartedTyping(username, room);

					if (!isTyping && UserStoppedTyping != null)
						UserStoppedTyping(username, room);
				}

			}
			else if ("stream-room-messages".Equals(collection) &&
			        obj["eventName"] != null)
			{
				var eventName = obj["eventName"].Value<string>();
				var eventArgs = obj["args"] as JArray;

				foreach (var message in eventArgs)
				{
					var msg = Message.Parse(_meteor, message as JObject);
					Debug.WriteLine("Message from: {0}", msg.RoomId);

					if (MessageReceived != null)
						MessageReceived(eventName, msg);
				}
			}
		}

		protected override void OnItemDeleted(string id, string collection)
		{
			//	Do nothing.
		}
	}
}
