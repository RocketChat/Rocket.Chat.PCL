using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using MeteorPCL;
using NUnit.Framework;

namespace RocketChatPCL.iOS.Tests
{
	[TestFixture]
	public class BasicUnitTest
	{
		private static string RocketChatHost = "";
		private static string RocketChatUser = "";
		private static string RocketChatPass = "";
		private static bool RocketChatSSL = true;
		private static int RocketChatPort = 443;

		/// <summary>
		/// The purpose of this unit test isn't really to test anything. Its to allow you to play with the library without
		/// deploying a full application. It will connect to the rocket chat server of your choosing, subscribe to every room
		/// and then print notifications to the console if users type, or add messages to the room.
		/// </summary>
		[Ignore]
		[Test]
		public async void RocketChatTest()
		{
			//	Instantiate the client.
			var rc = new RocketChatClient(RocketChatHost, RocketChatPort, RocketChatSSL);

			await rc.Connect(RocketChatUser, RocketChatPass);

			//	Subscribe to events.
			rc.Rooms.UserStartedTyping += (username, room) => Debug.WriteLine("User {0} is typing in room {1}", username, rc.Rooms[room].Name);
			rc.Rooms.UserStoppedTyping += (username, room) => Debug.WriteLine("User {0} is no longer typing in room {1}", username, rc.Rooms[room].Name);
			rc.Rooms.MessageReceived += (room, message) => Debug.WriteLine("New message received in room {0} from {1}: {2}", rc.Rooms[room].Name, message.User.Username, message.Text);

			//	Iterate over the rooms.
			foreach (var room in rc.Rooms.Keys)
			{
				var rm = rc.Rooms[room];
				var epoch = new DateTime(1970, 1, 1);
				var result = await rm.LoadHistory(epoch, 100, epoch);
				Debug.WriteLine("Got: {0} messages in the room {1}", rm.Name, result.Count);

				if (rm.Name != null)
					Debug.WriteLine("Room Id: {0} Name: {1}", rm.Id, rm.Name);
			}

			//	Sleep indefinitely to stop the test from aborting.
			while (true)
				Thread.Sleep(100);
		}
		 
	}
}
