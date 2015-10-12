using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
