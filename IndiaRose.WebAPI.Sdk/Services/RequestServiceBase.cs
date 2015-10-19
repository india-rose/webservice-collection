using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IndiaRose.WebAPI.Sdk.Interfaces;
using IndiaRose.WebAPI.Sdk.Models;

namespace IndiaRose.WebAPI.Sdk.Services
{
	public abstract class RequestServiceBase : IRequestService
	{
		protected HttpClient Client { get; private set; }

		protected RequestServiceBase(HttpMessageHandler messageHandler = null)
		{
			Client = messageHandler != null ? new HttpClient(messageHandler) : new HttpClient();
		}

		#region Get Methods

		public async Task<HttpResult> GetAsync(string uri, Dictionary<string, string> headers = null)
		{
			HttpResult result = new HttpResult();
			try
			{
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
				if (headers != null)
				{
					foreach (KeyValuePair<string, string> header in headers)
					{
						request.Headers.Add(header.Key, header.Value);
					}
				}

				using (HttpResponseMessage response = await Client.SendAsync(request))
				{
					result.StatusCode = response.StatusCode;
					result.Content = await response.Content.ReadAsStringAsync();
				}
			}
			catch (Exception e)
			{
				result.HasError = true;
				result.InnerException = e;
			}
			return result;
		}
		
		#endregion

		#region Post methods

		public Task<HttpResult> PostAsync(string uri, Dictionary<string, string> headers = null)
		{
			return PostAsync(uri, string.Empty, headers);
		}

		public async Task<HttpResult> PostAsync(string uri, string content, Dictionary<string, string> headers = null, string contentType = "application/json")
		{
			HttpResult result = new HttpResult();
			try
			{
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
				if (headers != null)
				{
					foreach (KeyValuePair<string, string> header in headers)
					{
						request.Headers.Add(header.Key, header.Value);
					}
				}

				if (!string.IsNullOrWhiteSpace(content))
				{
					request.Content = new StringContent(content, Encoding.UTF8, contentType);
				}

				using (HttpResponseMessage response = await Client.SendAsync(request))
				{
					result.StatusCode = response.StatusCode;
					result.Content = await response.Content.ReadAsStringAsync();
				}
			}
			catch (Exception e)
			{
				result.HasError = true;
				result.InnerException = e;
			}
			return result;
		}

		public async Task<HttpResult> PostAsync(string uri, Stream content, string contentType, Dictionary<string, string> headers = null)
		{
			HttpResult result = new HttpResult();
			try
			{
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
				if (headers != null)
				{
					foreach (KeyValuePair<string, string> header in headers)
					{
						request.Headers.Add(header.Key, header.Value);
					}
				}

				request.Content = new StreamContent(content);
				request.Content.Headers.Add("Content-Type", contentType);

				using (HttpResponseMessage response = await Client.SendAsync(request))
				{
					result.StatusCode = response.StatusCode;
					result.Content = await response.Content.ReadAsStringAsync();
				}
			}
			catch (Exception e)
			{
				result.HasError = true;
				result.InnerException = e;
			}
			return result;
		}

		#endregion
	}
}
