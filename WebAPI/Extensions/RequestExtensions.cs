﻿using System.Net;
using System.Net.Http;
using WebAPI.Helpers;

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
	}
}
