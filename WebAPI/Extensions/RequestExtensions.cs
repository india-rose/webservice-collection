using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using WebAPI.Common.Responses;

namespace WebAPI.Extensions
{
	public static class RequestExtensions
	{
		public static HttpResponseMessage CreateCustomError(this HttpRequestMessage request, int errorCode, string errorMessage)
		{
			return request.CreateResponse(HttpStatusCode.OK, new RequestResult
			{
				HasError = true,
				ErrorCode = errorCode,
				ErrorMessage = errorMessage
			});
		}

		public static HttpResponseMessage CreateGoodReponse<T>(this HttpRequestMessage request, T data)
		{
			return request.CreateResponse(HttpStatusCode.OK, new RequestResult<T>
			{
				HasError = false,
				ErrorCode = 0,
				ErrorMessage = "",
				Content = data
			});
		}

		public static HttpResponseMessage CreateEmptyGoodReponse(this HttpRequestMessage request)
		{
			return request.CreateResponse(HttpStatusCode.OK, new RequestResult
			{
				HasError = false,
				ErrorCode = 0,
				ErrorMessage = ""
			});
		}

		public static HttpResponseMessage CreateBadRequestResponse(this HttpRequestMessage request)
		{
			return request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing fields in request");
		}

		public static string GetHeaderValue(this HttpRequestMessage request, string headerName)
		{
			IEnumerable<string> resultHeader;
			if (request.Headers.TryGetValues(headerName, out resultHeader))
			{
				return resultHeader.FirstOrDefault();
			}
			return null;
		}
	}
}
