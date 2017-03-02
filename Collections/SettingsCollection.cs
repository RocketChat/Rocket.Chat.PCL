using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeteorPCL;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	public class SettingsCollection: AbstractCollection<object>, ISettingsCollection
	{

		public SettingsCollection(IMeteor meteor): base(meteor)
		{
		}

		public async Task Initialize(string userId, DateTime since)
		{
			var settings = await GetPublicSettingsSince(since);

			foreach (var setting in settings.Added)
				_items.Add(setting.Key, setting.Value);

			foreach (var setting in settings.Updated)
				_items[setting.Key] = setting.Value;

			foreach (var setting in settings.Removed)
			{
				if (_items.ContainsKey(setting.Key))
					_items.Remove(setting.Key);
			}

			await _meteor.Subscribe("stream-notify-all", new object[] { "public-settings-changed", false });
		
		}

		/// <summary>
		/// This method is used to retrieve the public settings, such as Site Name. 
		/// </summary>
		/// <returns>The public settings.</returns>
		public async Task<Dictionary<string, object>> GetPublicSettings()
		{
			var res = await _meteor.CallWithResult("public-settings/get", new object[] { });
			var output = new Dictionary<string, object>();

			if (res["result"] != null)
			{
			   output = TypeUtils.ParseKeyValuePairs(res["result"] as JArray);
			}
			return output;
		}

		/// <summary>
		/// This method is used to retrieve the public settings, such as Site Name. 
		/// It accepts a date as the first and only parameter which causes the results to be an object that 
		/// contains the updated and removed settings after the provided time. 
		/// </summary>
		/// <returns>The time after which settings should be returned.</returns>
		/// <param name="since">Since.</param>
		public async Task<CollectionDiff<KeyValuePair<string, object>>> GetPublicSettingsSince(DateTime since)
		{

			var res = await _meteor.CallWithResult("public-settings/get", new object[] { new Dictionary<string, object>() { { "$date", TypeUtils.DateTimeToTimestamp(since) } } });
			CollectionDiff<KeyValuePair<string, object>> output = new CollectionDiff<KeyValuePair<string, object>>();


			if (res == null)
			{
				return output;
			}

			var result = res["result"];

			if (result == null)
			{
				return output;
			}

			var updates = result["update"] != null ? (result as JObject)["update"] as JArray : null;
			var additions = result["add"] != null ? (result as JObject)["add"] as JArray : null;
			var removes = result["remove"] != null ? (result as JObject)["remove"] as JArray : null;

			if (additions != null)
			{
				foreach (var channelTok in additions)
				{
					output.Added.Add(TypeUtils.ParseKeyValuePair(channelTok as JObject));
				}
			}

			if (updates != null)
			{
				foreach (var channelTok in updates)
				{
					output.Updated.Add(TypeUtils.ParseKeyValuePair(channelTok as JObject));
				}
			}

			if (removes != null)
			{
				foreach (var channelTok in removes)
				{
					output.Removed.Add(TypeUtils.ParseKeyValuePair(channelTok as JObject));
				}
			}

			return output;
		}

		protected override void OnItemAdded(string id, string collection, JObject obj)
		{
			//	Do nothing.
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
