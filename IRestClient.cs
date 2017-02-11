using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RocketChatPCL
{
	/// <summary>
	/// Platform-agnostic Rest client interface.
	/// </summary>
	public interface IRestClient
	{
		/// <summary>
		/// Perform an HTTP GET request.
		/// </summary>
		/// <param name="uri">URI.</param>
		/// <param name="headers">Headers.</param>
		Task<RestResponse> get(String uri, Dictionary<string, string> headers);

		/// <summary>
		/// Perform an HTTP GET request.
		/// </summary>
		/// <returns>The get.</returns>
		/// <param name="uri">URI.</param>
		Task<RestResponse> get(string uri);

		/// <summary>
		/// Perform an HTTP POST request.
		/// </summary>
		/// <param name="uri">URI.</param>
		/// <param name="headers">Headers.</param>
		/// <param name="payload">Payload.</param>
		Task<RestResponse> post(String uri, Dictionary<string, string> headers, string payload);

		/// <summary>
		/// Perform an HTTP PUT request.
		/// </summary>
		/// <param name="uri">URI.</param>
		/// <param name="headers">Headers.</param>
		/// <param name="payload">Payload.</param>
		Task<RestResponse> put(String uri, Dictionary<string, string> headers, string payload);

		/// <summary>
		/// Perform an HTTP DELETE request.
		/// </summary>
		/// <param name="uri">URI.</param>
		/// <param name="headers">Headers.</param>
		Task<RestResponse> delete(String uri, Dictionary<string, string> headers);
	}
}
