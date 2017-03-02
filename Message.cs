using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MeteorPCL;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	public class Message
	{
		private IMeteor _meteor;

		public string Id { get; set; }
		public string RoomId { get; set; }
		public string Text { get; set; }
		public DateTime Created { get; set; }
		public User User { get; set; }
		public DateTime UpdatedAt { get; set; }
		public DateTime EditedAt { get; set; }
		public User EditedBy { get; set; }
		public List<AttachedUrl> Urls { get; set; }
		public List<Attachment> Attachments { get; set; }
		public string Alias { get; set; }
		public string Avatar { get; set; }
		public bool Groupable { get; set; }
		public bool ParseUrls { get; set; }
		public Bot Bot { get; set; }
		public Dictionary<string, List<string>> Reactions { get; set; }

		public Message(IMeteor meteor)
		{
			_meteor = meteor;
		}

		public static Message Parse(IMeteor meteor, JObject m)
		{
			Message message = new Message(meteor);

			if (m["_id"] != null)
				message.Id = (m["_id"] as JValue).Value<string>();

			if (m["rid"] != null)
				message.RoomId = (m["rid"] as JValue).Value<string>();

			if (m["msg"] != null)
				message.Text = (m["msg"] as JValue).Value<string>();

			if (m["_updatedAt"] != null)
				message.UpdatedAt = TypeUtils.ParseDateTime(m["_updatedAt"] as JObject);

			if (m["editedAt"] != null)
				message.EditedAt = TypeUtils.ParseDateTime(m["editedAt"] as JObject);

			if (m["editedBy"] != null)
				message.EditedBy = User.Parse(m["editedBy"] as JObject);

			message.Urls = new List<AttachedUrl>();

			if (m["urls"] != null)
				foreach (var url in m["urls"] as JArray)
					message.Urls.Add(AttachedUrl.Parse(url as JObject));

			message.Attachments = new List<Attachment>();
			if (m["attachments"] != null)
				foreach (var attachment in m["attachments"])
					message.Attachments.Add(Attachment.Parse(attachment as JObject));

			if (m["alias"] != null)
				message.Alias = m["alias"].Value<string>();

			if (m["avatar"] != null)
				message.Avatar = m["avatar"].Value<string>();

			if (m["groupable"] != null)
				message.Groupable = m["groupable"].Value<bool>();

			if (m["parseUrls"] != null)
				message.ParseUrls = m["parseUrls"].Value<bool>();

			if (m["u"] != null)
				message.User = User.Parse(m["u"] as JObject);

			if (m["bot"] != null)
				message.Bot = Bot.Parse(m["bot"] as JObject);

			message.Reactions = new Dictionary<string, List<string>>();
			if (m["reactions"] != null)
			{
				foreach (var reaction in (m["reactions"] as JObject))
				{
					message.Reactions.Add(reaction.Key, new List<string>());

					if (reaction.Value["usernames"] != null)
						foreach (var user in reaction.Value["usernames"])
							message.Reactions[reaction.Key].Add(user.Value<string>());
				}
			}

			return message;
		}

		public async Task<bool> Delete()
		{
			var a = await _meteor.CallWithResult("deleteMessage", new object[] {
				new Dictionary<string, object> { { "_id", Id } } });

			return a != null && a["msg"] != null && "result".Equals(a["msg"].Value<string>());
		}


		public async Task<Message> Update(string message)
		{
			var a = await _meteor.CallWithResult("updateMessage", new object[] {
				new Dictionary<string, object> { { "_id", Id }, { "rid", RoomId }, { "msg", message } } });

			return (a != null && a["result"] != null) ? Message.Parse(_meteor, a["result"] as JObject) : null;
		}

		public async Task<bool> Star()
		{
			return await SetMessageStarStatus(RoomId, Id, true);
		}

		public async Task<bool> Unstar()
		{
			return await SetMessageStarStatus(RoomId, Id, false);
		}

		public async Task<bool> React(string emoji)
		{
			var a = await _meteor.CallWithResult("setReaction", new object[] { emoji, Id });
			return a != null && a["msg"] != null && "result".Equals(a["msg"].Value<string>());
		}

		public async Task<Message> Pin()
		{
			var a = await _meteor.CallWithResult("pinMessage", new object[] { new Dictionary<string, object>() { { "_id", Id }, { "rid", RoomId } } });
			return a != null && a["result"] != null ? Message.Parse(_meteor, a["result"] as JObject) : null;
		}

		public async Task<Message> Unpin()
		{
			var a = await _meteor.CallWithResult("unpinMessage", new object[] { new Dictionary<string, object>() { { "_id", Id }, { "rid", RoomId } } });
			return a != null && a["result"] != null ? Message.Parse(_meteor, a["result"] as JObject) : null;
		}

		private async Task<bool> SetMessageStarStatus(string roomId, string messageId, bool starred)
		{
			var a = await _meteor.CallWithResult("starMessage", new object[] { new Dictionary<string, object> { { "_id", messageId }, { "rid", roomId }, { "starred", starred } } });
			return a != null && a["result"] != null && a["result"].Value<int>() == 1;
		}
	}
}
