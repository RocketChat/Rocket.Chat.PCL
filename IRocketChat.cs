using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RocketChatPCL
{
	public delegate void UserLoggedInEventArgs(string user, string token);
	public delegate void UserLoggedOutEventArgs(string user);
	
	public interface IRocketChat
	{
		Task<bool> Connect(string username, string password);
		/// <summary>
		/// This method call is used to get server-wide special users and their roles. 
		/// </summary>
		/// <returns>The user roles.</returns>
		Task<List<User>> GetUserRoles();
		/// <summary>
		/// Returns a list of custom emoji registered with the server.
		/// </summary>
		/// <returns>The custom emoji.</returns>
		Task<List<CustomEmoji>> ListCustomEmoji();
		/// <summary>
		/// Used to set the user presence status. 
		/// </summary>
		/// <returns>The default status.</returns>
		/// <param name="status">Status.</param>
		Task<bool> SetDefaultStatus(UserStatus status);
		/// <summary>
		/// Only away and online are accepted. This method call is useful when the client identifies that the user is 
		/// not using the application (and therefore away) and when he got back
		/// </summary>
		/// <returns>The temporary status.</returns>
		/// <param name="status">Status.</param>
		Task<bool> SetTemporaryStatus(UserStatus status);
	}
}
