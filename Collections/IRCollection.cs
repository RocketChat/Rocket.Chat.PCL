using System;
using System.Collections.Generic;

namespace RocketChatPCL
{
	public interface IRCollection<T>: IReadOnlyDictionary<string, T>
	{
	}
}
