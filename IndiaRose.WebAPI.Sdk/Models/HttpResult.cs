using System;
using System.Net;

namespace IndiaRose.WebAPI.Sdk.Models
{
	public class HttpResult
	{
		public HttpStatusCode StatusCode { get; set; }

		public string Content { get; set; }

		public bool HasError { get; set; }

		public Exception InnerException { get; set; }
	}
}
