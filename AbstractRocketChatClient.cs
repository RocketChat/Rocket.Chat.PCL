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
		private Task<string> GetRemoteVersion(string host, int port, bool ssl)
		{
			string url = string.Format("{0}://{1}:{2}/api/v1/info", _ssl ? "https" : "http", _host, _port);
			return _client.get(url).ContinueWith((versionTask) => {
				if (versionTask.Result.ResponseCode != 200) {
					return "0.0.0";
				}

				var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(versionTask.Result.ResponseText);

				if (dictionary.ContainsKey("version")) {
					return dictionary["version"].ToString();
				}

				return "0.0.0";
			});
		}

		/// <summary>
		/// Use the rocket chat REST API to log in.
		/// </summary>
		/// <returns>True if log in successful, false otherwise</returns>
		/// <param name="username">The username used to log into rocket chat.</param>
		/// <param name="password">The password used to log into rocket chat.</param>
		private Task<bool> DoLogin(string username, string password)
		{
			string url = string.Format("{0}://{1}:{2}/api/v1/login", _ssl ? "https" : "http", _host, _port);
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add("Content-Type", "application/json");

			var str = JsonConvert.SerializeObject(new UserLoginRequest() {
				Username = username,
				Password = password
			});

			var loginTask = _client.post(url, headers, str);

			return loginTask.ContinueWith((arg) => {
				if (loginTask.Result.ResponseCode == 401 || loginTask.Result.ResponseCode == 403 || 
				    loginTask.Result.ResponseCode == 500) {
					return false;
				}

				var loginResponse = JsonConvert.DeserializeObject<StandardResponse<SuccessfulAuthorization>>(loginTask.Result.ResponseText);

				if (loginResponse.Status.Equals("success")) {
					_userId = loginResponse.Data.UserId;
					_authToken = loginResponse.Data.AuthToken;

					_meteor.Connect(_host, _ssl);

					var meteorResult = _meteor.LoginWithToken(_authToken);

					meteorResult.Wait();

					//	TODO: Parse the response.

					return true;
				}
				return false;
			});
		}

		/// <summary>
		/// Connect to the rocket chat server with the specified username and password.
		/// </summary>
		/// <returns>True if the connection is successful and false otherwise</returns>
		/// <param name="username">The username used to log into rocket chat.</param>
		/// <param name="password">The password used to log into rocket chat.</param>
		public Task<bool> Connect(string username, string password)
		{
			return GetRemoteVersion(_host, _port, _ssl).ContinueWith((version) => {
				//	TODO: Check that the version is compatible.
				var login = DoLogin(username, password);
				login.Wait();

				if (!login.Result)
					return false;
				
				_meteor.Subscribe("stream-notify-user", new object[] { _userId + "/message", false }).Wait();
				_meteor.Subscribe("stream-notify-user", new object[] { _userId + "/otr", false }).Wait();
				_meteor.Subscribe("stream-notify-user", new object[] { _userId + "/webrtc", false }).Wait();
				_meteor.Subscribe("stream-notify-user", new object[] { _userId + "/notification", false }).Wait();
				_meteor.Subscribe("stream-notify-user", new object[] { _userId + "/subscriptions-changed", false }).Wait();
				_meteor.Subscribe("stream-notify-user", new object[] { _userId + "/rooms-changed", false }).Wait();

				_meteor.MessageReceived += (message) => Debug.WriteLine("Message received: {0}", message);

				//	Prepopulate the system with some meta-data from the server
				//	Including the current list of users rooms/subscriptions/permissions and the server settings.
				var rms = Rooms.Initialize(_userId, TypeUtils.UnixEpoch);
				var sets = Settings.Initialize(_userId, TypeUtils.UnixEpoch);
				var perms = Permissions.Initialize(_userId, TypeUtils.UnixEpoch);

				//	Wait for them to finish before returning.
				rms.Wait();
				sets.Wait();
				perms.Wait();

				_meteor.Subscribe("userData").Wait();
				_meteor.Subscribe("activeUsers").Wait();
				return true;
			});
		}

		/// <summary>
		/// This method call is used to get server-wide special users and their roles. 
		/// </summary>
		/// <returns>The user roles.</returns>
		public Task<List<User>> GetUserRoles()
		{
			return _meteor.CallWithResult("getUserRoles", new object[] { })
						  .ContinueWith((arg) => {
				List<User> users = new List<User>();
			   	var result = arg.Result["result"] as JArray;

				foreach (var user in result)
					users.Add(User.Parse(user as JObject));
			   
			   	return users;
			});
		}

		/// <summary>
		/// Returns a list of custom emoji registered with the server.
		/// </summary>
		/// <returns>The custom emoji.</returns>
		public Task<List<CustomEmoji>> ListCustomEmoji()
		{
			return _meteor.CallWithResult("listEmojiCustom", new object[] { })
						   .ContinueWith((arg) =>
			{
				List<CustomEmoji> users = new List<CustomEmoji>();
				var result = arg.Result["result"] as JArray;

				foreach (var user in result)
					users.Add(CustomEmoji.Parse(user as JObject));
			   
				return users;
			});
		}

		/// <summary>
		/// Used to set the user presence status. 
		/// </summary>
		/// <returns>The default status.</returns>
		/// <param name="status">Status.</param>
		public Task<bool> SetDefaultStatus(UserStatus status)
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
			return _meteor.CallWithResult("UserPresence:setDefaultStatus", new object[] { userStatus })
						  .ContinueWith((a) => a.Result != null && a.Result["msg"] != null && "result".Equals(a.Result["msg"].Value<string>()));
			
		}

		/// <summary>
		/// Only away and online are accepted. This method call is useful when the client identifies that the user is 
		/// not using the application (and therefore away) and when he got back
		/// </summary>
		/// <returns>The temporary status.</returns>
		/// <param name="status">Status.</param>
		public Task<bool> SetTemporaryStatus(UserStatus status)
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
			return _meteor.CallWithResult("UserPresence:" + userStatus, new object[] { userStatus })
						   .ContinueWith((arg) => arg.Result != null && arg.Result["msg"] != null && "result".Equals(arg.Result["msg"].Value<string>()));
		}
	}
}
