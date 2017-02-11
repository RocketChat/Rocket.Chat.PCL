using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RocketChatPCL
{
	public class RestClient: IRestClient
	{
		private HttpClient httpClient;

		public RestClient()
		{
			this.httpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromSeconds(30)
			};
		}

		public Task<RestResponse> delete(string uri, Dictionary<string, string> headers)
		{
			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri(uri),
				Method = HttpMethod.Delete
			};

			return execute(request, headers);
		}

		public Task<RestResponse> get(string uri)
		{
			return get(uri, new Dictionary<string, string>());
		}

		public Task<RestResponse> get(string uri, Dictionary<string, string> headers)
		{
			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri(uri),
				Method = HttpMethod.Get
			};

			return execute(request, headers);
		}

		public Task<RestResponse> post(string uri, Dictionary<string, string> headers, string payload)
		{
			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri(uri),
				Method = HttpMethod.Post,
				Content = new StringContent(payload, Encoding.UTF8, "application/json")
			};

			return execute(request, headers);
		}

		public Task<RestResponse> put(string uri, Dictionary<string, string> headers, string payload)
		{
			var request = new HttpRequestMessage()
			{
				RequestUri = new Uri(uri),
				Method = HttpMethod.Put,
				Content = new StringContent(payload, Encoding.UTF8, "application/json")
			};

			return execute(request, headers);
		}

		private Task<RestResponse> execute(HttpRequestMessage request, Dictionary<string, string> headers)
		{
			foreach (KeyValuePair<string, string> header in headers)
			{
				request.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			return httpClient.SendAsync(request).ContinueWith((res) =>
			{
				try
				{
					var a = res.Result.Content.ReadAsStringAsync().ContinueWith(text =>
					{
						return new RestResponse(text.Result, (int)res.Result.StatusCode);
					});

					a.Wait();

					return a.Result;
				}
				catch
				{
					return new RestResponse("Communication failure", 500);
				}
			});
		}

	}
}
