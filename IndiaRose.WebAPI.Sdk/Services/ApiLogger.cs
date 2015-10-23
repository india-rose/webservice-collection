using System;
using System.Diagnostics;
using IndiaRose.WebAPI.Sdk.Interfaces;

namespace IndiaRose.WebAPI.Sdk.Services
{
	public class ApiLogger : IApiLogger
	{
		public void LogRequest(string message)
		{
			Debug.WriteLine("ApiLogger:LogRequest : {0}", message);
		}

		public void LogError(string request, Exception error)
		{
			Debug.WriteLine("ApiLogger:LogError : {0} => {1}\n{2}", request, error.Message, error.StackTrace);
		}

		public void LogResultError(string request, int errorCode, string errorMessage)
		{
			Debug.WriteLine("ApiLogger:LogResultError : {0} => {1} / {2}", request, errorCode, errorMessage);
		}

		public void LogServerError(string request, string message)
		{
			Debug.WriteLine("ApiLogger:LogServerError : {0} => {1}", request, message);
		}
	}
}
