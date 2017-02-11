using System;
using System.Collections.Generic;

namespace RocketChatPCL
{
	public interface IUserCollection: IReadOnlyDictionary<string, User>
	{
		event UserStatusChangedEventArgs UserStatusChanged;
	}
}
