using System;
namespace RocketChatPCL
{
	public class RestResponse
	{
		private string _responseText;
		private int _responseCode;
		public RestResponse(string responseText, int responseCode)
		{
			this._responseText = responseText;
			this._responseCode = responseCode;
		}

		public string ResponseText
		{
			get { return _responseText; }
		}

		public int ResponseCode
		{
			get { return _responseCode; }
		}
	}
}
