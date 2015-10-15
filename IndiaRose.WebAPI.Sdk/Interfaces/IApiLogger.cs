using System;

namespace IndiaRose.WebAPI.Sdk.Interfaces
{
	public interface IApiLogger
	{
		void LogRequest(string message);

		void LogError(string request, Exception error);

		void LogResultError(string request, int errorCode, string errorMessage);

		void LogServerError(string request, string message);
	}
}
