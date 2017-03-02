using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MeteorPCL;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	public class Room
	{
		private IMeteor _meteor;

		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public User Owner { get; set; }
		public string Topic { get; set; }
		public bool Default { get; set; }
		public bool ReadOnly { get; set; }
		public RoomType Type { get; set; }
		public List<string> MutedUsers { get; set; }
		public DateTime DeletedAt { get; set; }

		public Room(IMeteor meteor)
		{
			_meteor = meteor;
		}

		/// <summary>
		/// Use this method to make the initial load of a room. After the initial load you may subscribe to the room messages stream.
		/// </summary>
		/// <returns>The history.</returns>
		/// <param name="roomId">The room id</param>
		/// <param name="oldestMessage">the oldest message date  - this is used to do pagination</param>
		/// <param name="quantity">The message quantity</param>
		/// <param name="lastUpdate">A date object - the date of the last time the client got data for the room</param>
		public async Task<List<Message>> LoadHistory(DateTime oldestMessage, int quantity, DateTime lastUpdate)
		{
			var oldest = TypeUtils.DateTimeToTimestamp(oldestMessage);
			var updated = TypeUtils.DateTimeToTimestamp(lastUpdate);

			var oldestObj = (oldest == 0) ? null : new Dictionary<string, object>() { { "$date", oldest } };
			var updatedObj = new Dictionary<string, object>() { { "$date", updated } };

			var obj = await _meteor.CallWithResult("loadHistory", new object[] { Id, oldestObj, quantity, updatedObj });
			var response = new List<Message>();

			if (obj != null && obj["result"] != null && obj["result"]["messages"] != null)
				foreach (var message in obj["result"]["messages"] as JArray)
					response.Add(Message.Parse(_meteor, message as JObject));
			
			return response;
		}

		/// <summary>
		/// Send a text message
		/// </summary>
		/// <returns>The message.</returns>
		/// <param name="roomId">The room id for where to send this message</param>
		/// <param name="message">The message body (the text of the message itself)</param>
		public async Task<Message> SendMessage(string message)
		{
			var arg = await _meteor.CallWithResult("sendMessage", new object[] { new Dictionary<string, object> { { "rid", Id }, { "msg", message } } });
			return arg != null && arg["result"] != null ? Message.Parse(_meteor, arg["result"] as JObject) : null;
		}

		/// <summary>
		/// This method call is used to get room-wide special users and their roles. You may send an collection of room id (at least one).
		/// </summary>
		/// <returns> collection of users and its roles per room.</returns>
		public async Task<List<RoomRole>> GetRoles()
		{
			var res = await _meteor.CallWithResult("getRoomRoles", new object[] { Id });
			var roles = new List<RoomRole>();
			if (res["result"] != null)
			foreach (var role in res["result"] as JArray)
				roles.Add(RoomRole.Parse(role as JObject));
			
		   	return roles;
		}
		/// <summary>
		/// Deletes the room.
		/// </summary>
		/// <returns>True on success, false on failure.</returns>
		public async Task<bool> Delete()
		{
			var arg = await _meteor.CallWithResult("eraseRoom", new object[] { Id });
			return arg != null && arg["result"] != null && arg["result"].Value<int>() == 1;
		}

		/// <summary>
		/// Archives the room.
		/// </summary>
		/// <returns>True on success, false on failure.</returns>
		public async Task<bool> Archive()
		{
			var arg = await _meteor.CallWithResult("archiveRoom", new object[] { Id });
			return arg != null && arg["msg"] != null && "result".Equals(arg["result"].Value<string>());
		}

		/// <summary>
		/// Unarchives the room.
		/// </summary>
		/// <returns>True on success, false on failure.</returns>
		public async Task<bool> Unarchive()
		{
			var arg = await _meteor.CallWithResult("unarchiveRoom", new object[] { Id });
			return arg != null && arg["msg"] != null && "result".Equals(arg["result"].Value<string>());
		}

		/// <summary>
		/// Join a public channel (optionally providing a join code).
		/// </summary>
		/// <returns>The channel.</returns>
		/// <param name="joinCode">Optional Join code.</param>
		public async Task<bool> Join(string joinCode = null)
		{
			object[] args;

			args = joinCode != null ? new object[] { Id, joinCode } : new object[] { Id };
			var arg = await _meteor.CallWithResult("joinRoom", args);
			return arg != null && arg["result"] != null && arg["result"].Value<bool>();
		}

		/// <summary>
		/// Leaves the channel.
		/// </summary>
		/// <returns>The channel.</returns>
		public async Task<bool> Leave()
		{
			var arg = await _meteor.CallWithResult("leaveRoom", new object[] { Id });
			return arg != null && arg["msg"] != null && "result".Equals(arg["result"].Value<string>());
		}
		/// <summary>
		/// Hides the room.
		/// </summary>
		/// <returns>The room.</returns>
		public async Task<bool> Hide()
		{
			var arg = await _meteor.CallWithResult("hideRoom", new object[] { Id });
			return arg != null && arg["result"] != null && arg["result"].Value<int>() == 1;
		}
		/// <summary>
		/// Opens the room.
		/// </summary>
		/// <returns>The room.</returns>
		public async Task<bool> Open()
		{
			var arg = await _meteor.CallWithResult("openRoom", new object[] { Id });
			return arg != null && arg["result"] != null && arg["result"].Value<int>() == 1;
		}
		/// <summary>
		/// Favourites the room.
		/// </summary>
		/// <returns>The room.</returns>
		public Task<bool> Favourite()
		{
			return SetRoomFavouriteStatus(Id, true);
		}
		/// <summary>
		/// Unfavourites the room.
		/// </summary>
		/// <returns>The room.</returns>
		public Task<bool> Unfavourite()
		{
			return SetRoomFavouriteStatus(Id, false);
		}
		/// <summary>
		/// Saves a room setting.
		/// </summary>
		/// <returns>The room setting.</returns>
		/// <param name="setting">Setting.</param>
		/// <param name="value">Value.</param>
		public async Task<bool> SaveSetting(string setting, object value)
		{
			var arg = await _meteor.CallWithResult("saveRoomSettings",
												   new object[] { Id, setting, value });
			return arg != null && 
				   arg["result"] != null && 
				   arg["result"]["result"] != null && 
				   arg["result"]["result"].Value<bool>();
		}

		private async Task<bool> SetRoomFavouriteStatus(string roomId, bool favourite)
		{
			var arg = await _meteor.CallWithResult("toggleFavorite", new object[] { roomId, favourite });
			return arg != null && arg["result"] != null && arg["result"].Value<int>() == 1;
		}

		public async Task Subscribe()
		{
			//	Subscribe to typing notifications.
			await _meteor.Subscribe("stream-notify-room", new object[] { Id + "/typing", false });

			//	Subscribe to delete message notifications
			await _meteor.Subscribe("stream-notify-room", new object[] { Id + "/deleteMessage", false });

			//	Subscribe to the room message notifications
			await _meteor.Subscribe("stream-room-messages", new object[] { Id, false });

			Debug.WriteLine("Subscribed to Room {0} ({1})", Name, Id);

		}

		public static Room Update(Room existing, Room updated)
		{
			Room room = new Room(existing._meteor);

			room.Id = updated.Id != null ? updated.Id : existing.Id;
			room.Name = updated.Name != null ? updated.Name : existing.Name;
			room.Description = updated.Description != null ? updated.Description : existing.Description;
			room.Owner = updated.Owner != null ? updated.Owner : existing.Owner;
			room.Topic = updated.Topic != null ? updated.Topic : existing.Topic;
			room.Default = updated.Default;
			room.ReadOnly = updated.ReadOnly;
			room.Type = updated.Type;
			room.MutedUsers = updated.MutedUsers != null ? updated.MutedUsers : existing.MutedUsers;
			room.DeletedAt = updated.DeletedAt;

			return room;
		}

		public static Room Update(Room existing, Subscription updated)
		{
			Room room = new Room(existing._meteor);

			room.Id = updated.RoomId != null ? updated.RoomId : existing.Id;
			room.Name = updated.Name != null ? updated.Name : existing.Name;
			//room.Description = updated.Description != null ? updated.Description : existing.Description;
			//room.Owner = updated.Owner != null ? updated.Owner : existing.Owner;
			//room.Topic = updated.Topic != null ? updated.Topic : existing.Topic;
			//room.Default = updated.Default;
			//room.ReadOnly = updated.ReadOnly;
			room.Type = updated.Type;
			//room.MutedUsers = updated.MutedUsers != null ? updated.MutedUsers : existing.MutedUsers;
			//room.DeletedAt = updated.DeletedAt;


			return room;
		}

		public static Room Parse(IMeteor meteor, JObject m)
		{
			var room = new Room(meteor);
			if (m["_id"] != null)
				room.Id = m["_id"].Value<string>();

			if (m["name"] != null)
				room.Name = m["name"].Value<string>();

			if (m["t"] != null) {
				switch (m["t"].ToString()) {
				case "d":
					room.Type = RoomType.DirectMessage;
					break;
				case "p":
					room.Type = RoomType.PrivateGroup;
					break;
				case "c":
					room.Type = RoomType.PublicChannel;
					break;
				}
			}

			if (m["u"] != null)
				room.Owner = User.Parse(m["u"] as JObject);

			if (m["topic"] != null)
				room.Topic = m["topic"].Value<string>();

			if (m["default"] != null)
				room.Default = m["default"].Value<bool>();

			if (m["ro"] != null)
				room.ReadOnly = m["ro"].Value<bool>();

			if (m["description"] != null)
				room.Description = m["description"].Value<string>();

			room.MutedUsers = new List<string>();
			if (m["muted"] != null)
				foreach (var user in m["muted"] as JArray)
					room.MutedUsers.Add(user.Value<string>());

			if (m["_deletedAt"] != null)
				room.DeletedAt = TypeUtils.ParseDateTime(m["_deletedAt"] as JObject);

			return room;
		}
	}
}
