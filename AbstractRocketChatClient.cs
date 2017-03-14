using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using MeteorPCL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	/// <summary>
	/// This is where most of the implementation for the rocket chat client goes.
	/// Most of the logic should be in this class, with the platform specific stuff
	/// defined in the implementation.
	/// </summary>
	public abstract class AbstractRocketChatClient : IRocketChat
	{
		private string _host, _userId, _authToken;
		private int _port;
		private bool _ssl;

		private IMeteor _meteor;
		private IRestClient _client;

		//	Static strings for room settings.
		public static string RM_SETTING_ROOM_NAME = "roomName";
		public static string RM_SETTING_ROOM_TOPIC = "roomTopic";
		public static string RM_SETTING_ROOM_DESCRIPTION = "roomDescription";
		public static string RM_SETTING_ROOM_TYPE = "roomType";
		public static string RM_SETTING_READ_ONLY = "readOnly";
		public static string RM_SETTING_SYSTEM_MSGS = "systemMessages";
		public static string RM_SETTING_DEFAULT = "default";
		public static string RM_SETTING_JOIN_CODE = "joinCode";

		//	Maintained collections.
		public UserCollection Users { get; }
		public RoomCollection Rooms { get; }
		public SettingsCollection Settings { get; }
		public PermissionsCollection Permissions { get; }

		public string AuthToken { 
			get { return _authToken; } 
		}
		public string UserId { 
			get { return _userId; } 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:RocketChatPCL.AbstractRocketChatClient"/> class.
		/// </summary>
		/// <param name="host">The host name of the rocket chat server</param>
		/// <param name="port">The port number of the rocket chat server</param>
		/// <param name="ssl">If set to <c>true</c> https and wss will be used.</param>
		/// <param name="restClient">The platform specific rest client implementation</param>
		/// <param name="meteor">The MeteorPCL implementation</param>
		public AbstractRocketChatClient(string host, int port, bool ssl, IRestClient restClient, IMeteor meteor)
		{
			_ssl = ssl;
			_host = host;
			_port = port;
			_meteor = meteor;
			_client = restClient;

			Users = new UserCollection (_meteor);
			Rooms = new RoomCollection (_meteor);
			Settings = new SettingsCollection (_meteor);
			Permissions = new PermissionsCollection (_meteor);
		}

		/// <summary>
		/// Ping the Rocket Chat API and retrieve the version number.
		/// </summary>
		/// <returns>The remote version.</returns>
		/// <param name="host">The host name of the rocket chat server</param>
		/// <param name="port">The port number of the rocket chat server</param>
		/// <param name="ssl">If set to <c>true</c> https will be used.</param>
		private async Task<string> GetRemoteVersion(string host, int port, bool ssl)
		{
			string url = string.Format("{0}://{1}:{2}/api/v1/info", _ssl ? "https" : "http", _host, _port);
			var versionTask = await _client.get(url);
				
			if (versionTask.ResponseCode != 200) {
				return "0.0.0";
			}

			var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(versionTask.ResponseText);

			if (dictionary.ContainsKey("version")) {
				return dictionary["version"].ToString();
			}

			return "0.0.0";
		}

		/// <summary>
		/// Use the rocket chat REST API to log in.
		/// </summary>
		/// <returns>True if log in successful, false otherwise</returns>
		/// <param name="username">The username used to log into rocket chat.</param>
		/// <param name="password">The password used to log into rocket chat.</param>
		private async Task<bool> DoLogin(string username, string password)
		{
			string url = string.Format("{0}://{1}:{2}/api/v1/login", _ssl ? "https" : "http", _host, _port);
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add("Content-Type", "application/json");

			var str = JsonConvert.SerializeObject(new UserLoginRequest() {
				Username = username,
				Password = password
			});

			var loginTask = await _client.post(url, headers, str);

			if (loginTask.ResponseCode == 401 || loginTask.ResponseCode == 403 || 
			    loginTask.ResponseCode == 500) {
				return false;
			}

			var loginResponse = JsonConvert.DeserializeObject<StandardResponse<SuccessfulAuthorization>>(loginTask.ResponseText);

			if (loginResponse.Status.Equals("success")) {
				_userId = loginResponse.Data.UserId;
				_authToken = loginResponse.Data.AuthToken;

				var meUrl = string.Format("{0}://{1}:{2}/api/v1/me", _ssl ? "https" : "http", _host, _port);
				Dictionary<string, string> meHeaders = new Dictionary<string, string>();
				meHeaders.Add("Accept", "application/json");
				meHeaders.Add("X-Auth-Token", _authToken);
				meHeaders.Add("X-User-Id", _userId);

				var meTask = await _client.get(meUrl, meHeaders);

				if (meTask.ResponseCode != 200)
				{
					return false;
				}

				var meResponse = JsonConvert.DeserializeObject<MeResponse>(meTask.ResponseText);


				User user = new User();
				user.Id = _userId;
				user.Name = meResponse.Name;
				user.Username = meResponse.Username;
				user.UtcOffset = meResponse.UtcOffset;

				Users.Add(user);

				_meteor.Connect(_host, _ssl);

				await _meteor.LoginWithToken(_authToken);

				return true;
			}
			return false;

		}

		/// <summary>
		/// Connect to the rocket chat server with the specified username and password.
		/// </summary>
		/// <returns>True if the connection is successful and false otherwise</returns>
		/// <param name="username">The username used to log into rocket chat.</param>
		/// <param name="password">The password used to log into rocket chat.</param>
		public async Task<bool> Connect(string username, string password)
		{
			await GetRemoteVersion(_host, _port, _ssl);

			//	TODO: Check that the version is compatible.
			var login = await DoLogin(username, password);

			if (!login)
				return false;
				
			await _meteor.Subscribe("stream-notify-user", new object[] { _userId + "/message", false });
			await _meteor.Subscribe("stream-notify-user", new object[] { _userId + "/otr", false });
			await _meteor.Subscribe("stream-notify-user", new object[] { _userId + "/webrtc", false });
			await _meteor.Subscribe("stream-notify-user", new object[] { _userId + "/notification", false });
			await _meteor.Subscribe("stream-notify-user", new object[] { _userId + "/subscriptions-changed", false });
			await _meteor.Subscribe("stream-notify-user", new object[] { _userId + "/rooms-changed", false });

			_meteor.MessageReceived += (message) => Debug.WriteLine("Message received: {0}", message);

			//	Prepopulate the system with some meta-data from the server
			//	Including the current list of users rooms/subscriptions/permissions and the server settings.
			await Rooms.Initialize(_userId, TypeUtils.UnixEpoch);
			await Settings.Initialize(_userId, TypeUtils.UnixEpoch);
			await Permissions.Initialize(_userId, TypeUtils.UnixEpoch);

			await _meteor.Subscribe("userData");
			await _meteor.Subscribe("activeUsers");
			return true;
		}

		/// <summary>
		/// This method call is used to get server-wide special users and their roles. 
		/// </summary>
		/// <returns>The user roles.</returns>
		public async Task<List<User>> GetUserRoles()
		{
			var arg = await _meteor.CallWithResult("getUserRoles", new object[] { });
			List<User> users = new List<User>();
		   	var result = arg["result"] as JArray;

			foreach (var user in result)
				users.Add(User.Parse(user as JObject));
		   
		   	return users;
		}

		/// <summary>
		/// Returns a list of custom emoji registered with the server.
		/// </summary>
		/// <returns>The custom emoji.</returns>
		public async Task<List<CustomEmoji>> ListCustomEmoji()
		{
			var arg = await _meteor.CallWithResult("listEmojiCustom", new object[] { });

			List<CustomEmoji> users = new List<CustomEmoji>();
			var result = arg["result"] as JArray;

			foreach (var user in result)
				users.Add(CustomEmoji.Parse(user as JObject));
		   
			return users;
		}

		/// <summary>
		/// Used to set the user presence status. 
		/// </summary>
		/// <returns>The default status.</returns>
		/// <param name="status">Status.</param>
		public async Task<bool> SetDefaultStatus(UserStatus status)
		{
			string userStatus = "online";
			switch (status) {
			case UserStatus.Away:
				userStatus = "away";
				break;
			case UserStatus.Busy:
				userStatus = "busy";
				break;
			case UserStatus.Online:
				userStatus = "online";
				break;
			case UserStatus.Offline:
				userStatus = "offline";
				break;
			}
			var a = await _meteor.CallWithResult("UserPresence:setDefaultStatus", new object[] { userStatus });
			return a != null && a["msg"] != null && "result".Equals(a["msg"].Value<string>());
		}

		/// <summary>
		/// Only away and online are accepted. This method call is useful when the client identifies that the user is 
		/// not using the application (and therefore away) and when he got back
		/// </summary>
		/// <returns>The temporary status.</returns>
		/// <param name="status">Status.</param>
		public async Task<bool> SetTemporaryStatus(UserStatus status)
		{
			string userStatus = "online";
			switch (status) {
			case UserStatus.Away:
				userStatus = "away";
				break;
			case UserStatus.Online:
				userStatus = "online";
				break;
			}
			var arg = await _meteor.CallWithResult("UserPresence:" + userStatus, new object[] { userStatus });
			return arg != null && arg["msg"] != null && "result".Equals(arg["msg"].Value<string>());
		}

		public async Task Disconnect()
		{
			_meteor.Disconnect();
		}
	}
}
