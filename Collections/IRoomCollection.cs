using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RocketChatPCL
{
	public delegate void UserTypingEventArgs(string username, string room);
	public delegate void RoomMessageReceivedEventArgs(string room, Message message);
	public delegate void RoomUpdatedEventArgs(Room room);

	public interface IRoomCollection: IRCollection<Room>
	{
		event UserTypingEventArgs UserStartedTyping;
		event UserTypingEventArgs UserStoppedTyping;
		event RoomMessageReceivedEventArgs MessageReceived;
		event RoomUpdatedEventArgs RoomUpdated;
		/// <summary>
		/// This is the method call used to get all the rooms a user belongs to. 
		/// It accepts a timestamp with the latest client update time in order to just send what changed since last 
		/// call. If it’s the first time calling, just send a 0 as date.
		/// </summary>
		/// <returns>The rooms.</returns>
		/// <param name="since">date with the latest client update time in order to just send what changed since last 
		/// call. If it’s the first time calling, just send a Epoch as date.</param>
		Task<CollectionDiff<Room>> GetRooms(DateTime since);
		/// <summary>
		/// Create a new public channel.
		/// </summary>
		/// <returns>The channel.</returns>
		/// <param name="name">The name of the channel to create.</param>
		/// <param name="participants">List of usernames of participants.</param>
		/// <param name="readOnly">If set to <c>true</c> read only.</param>
		Task<Room> CreateChannel(string name, List<string> participants, bool readOnly);
		/// <summary>
		/// Create a new private group.
		/// </summary>
		/// <returns>The channel.</returns>
		/// <param name="name">The name of the channel to create.</param>
		/// <param name="participants">List of usernames of participants.</param>
		/// <param name="readOnly">If set to <c>true</c> read only.</param>
		Task<Room> CreatePrivateGroup(string name, List<string> participants, bool readOnly);
	}
}
