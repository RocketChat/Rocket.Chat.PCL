using System;
using MeteorPCL;

namespace RocketChatPCL
{
	public class RocketChatClient: AbstractRocketChatClient
	{
		public RocketChatClient(string host, int port, bool ssl) :
			base(host, port, ssl, new RestClient(), new Meteor())
		{

		}
	}
}
