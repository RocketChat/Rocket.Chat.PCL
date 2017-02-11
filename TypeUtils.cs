using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	public class TypeUtils
	{
		public static DateTime UnixEpoch = new DateTime(1970, 1, 1);
		public static DateTime ParseDateTime(JObject m)
		{
			DateTime dt = new DateTime(1970, 1, 1);

			return (m["$date"] != null) ? dt.AddMilliseconds(m["$date"].Value<long>()) : dt;
		}

		public static long DateTimeToTimestamp(DateTime dt)
		{
			double ms = dt.Subtract(UnixEpoch).TotalMilliseconds;
			return (long) ((ms >= 0) ? ms : 0);
		}

		public static Dictionary<string, object> ParseKeyValuePairs(JArray m)
		{
			var output = new Dictionary<string, object>();
			foreach (var kvp in m)
			{
				var k = ParseKeyValuePair(kvp as JObject);
				output.Add(k.Key, k.Value);
			}

			return output;
		}

		public static KeyValuePair<string, object> ParseKeyValuePair(JObject m)
		{
			string key = null;
			object val = null;

			if (m["_id"] != null)
				key = m["_id"].Value<string>();

			if (m["value"] != null)
				val = m["value"].Value<object>();

			return new KeyValuePair<string, object>(key, val);
		}
	}
}
