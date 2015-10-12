using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IndiaRose.WebAPI.Sdk.Models;

namespace IndiaRose.WebAPI.Sdk.Interfaces
{
	public interface IRequestService
	{
		Task<HttpResult> GetAsync(string uri, Dictionary<string, string> headers = null);

		Task<HttpResult> PostAsync(string uri, Dictionary<string, string> headers = null);

		Task<HttpResult> PostAsync(string uri, string content, Dictionary<string, string> headers = null);

		Task<HttpResult> PostAsync(string uri, Stream content, Dictionary<string, string> headers = null);
	}
}
