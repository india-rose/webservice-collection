using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using IndiaRose.WebAPI.Sdk.Interfaces;
using IndiaRose.WebAPI.Sdk.Models;
using IndiaRose.WebAPI.Sdk.Results;
using Newtonsoft.Json;
using WebAPI.Common.Requests;
using WebAPI.Common.Responses;

namespace IndiaRose.WebAPI.Sdk.Services
{
	public class ApiService : IApiService
	{
		private readonly string _apiHost;
		private readonly IRequestService _requestService;
		private readonly IApiLogger _apiLogger;

		public ApiService(IRequestService requestService, IApiLogger logger, string apiHost)
		{
			_requestService = requestService;
			_apiLogger = logger;
			_apiHost = apiHost;
		}

		public async Task<UserStatusCode> RegisterUserAsync(string login, string email, string password)
		{
			string requestDescription = string.Format("RegisterUserAsync({0}, {1}, {2})", login, email, password);
			_apiLogger.LogRequest(requestDescription);

			string requestContent = JsonConvert.SerializeObject(new UserRegisterRequest
			{
				Login = login,
				Email = email,
				Password = password
			});

			HttpResult result = await _requestService.PostAsync(_apiHost + Uris.USER_REGISTER, requestContent, DefaultHeaders());

			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return UserStatusCode.InternalError;
			}
			if (result.StatusCode == HttpStatusCode.OK)
			{
				try
				{
					RequestResult requestResult = JsonConvert.DeserializeObject<RequestResult>(result.Content);

					if (requestResult.HasError)
					{
						_apiLogger.LogResultError(requestDescription, requestResult.ErrorCode, requestResult.ErrorMessage);
						switch (requestResult.ErrorCode)
						{
							case 100:
								return UserStatusCode.LoginAlreadyExists;
							case 101:
								return UserStatusCode.EmailAlreadyExists;
							default:
								return UserStatusCode.UnknownError;
						}
					}
					return UserStatusCode.Ok;
				}
				catch (Exception)
				{
					_apiLogger.LogServerError(requestDescription, string.Format("Invalid result json : {0}", result.Content));
				}
			}
			if (result.StatusCode == HttpStatusCode.BadRequest)
			{
				return UserStatusCode.BadRequest;
			}
			_apiLogger.LogServerError(requestDescription, result.Content);
			return UserStatusCode.InternalError;
		}

		public async Task<UserStatusCode> LoginUserAsync(string login, string password)
		{
			string requestDescription = string.Format("LoginUserAsync({0}, {1})", login, password);
			_apiLogger.LogRequest(requestDescription);

			string requestContent = JsonConvert.SerializeObject(new UserLoginRequest
			{
				Login = login,
				Password = password
			});

			HttpResult result = await _requestService.PostAsync(_apiHost + Uris.USER_LOGIN, requestContent, DefaultHeaders());

			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return UserStatusCode.InternalError;
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					return UserStatusCode.Ok;
				case HttpStatusCode.BadRequest:
					return UserStatusCode.BadRequest;
				case HttpStatusCode.Unauthorized:
					return UserStatusCode.InvalidLoginOrPassword;
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return UserStatusCode.InternalError;
		}

		private Dictionary<string, string> DefaultHeaders()
		{
			return new Dictionary<string, string>
			{
				{"Accept", "application/json"}
			};
		}
	}
}
