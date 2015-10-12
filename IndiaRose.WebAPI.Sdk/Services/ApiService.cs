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

		public async Task<DeviceStatusCode> CreateDeviceAsync(UserInfo user, string deviceName)
		{
			string requestDescription = string.Format("CreateDeviceAsync(({0}, {1}), {2})", user.Login, user.Password, deviceName);
			_apiLogger.LogRequest(requestDescription);

			string requestContent = JsonConvert.SerializeObject(new DeviceCreateRequest
			{
				Name = deviceName
			});

			HttpResult result = await _requestService.PostAsync(_apiHost + Uris.DEVICE_CREATE, requestContent, UserHeaders(user));

			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return DeviceStatusCode.InternalError;
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.Created:
					return DeviceStatusCode.Ok;
				case HttpStatusCode.BadRequest:
					return DeviceStatusCode.BadRequest;
				case HttpStatusCode.Conflict:
					return DeviceStatusCode.DeviceAlreadyExists;
				case HttpStatusCode.Unauthorized:
					return DeviceStatusCode.InvalidLoginOrPassword;
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return DeviceStatusCode.InternalError;

		}

		public async Task<DeviceStatusCode> RenameDeviceAsync(UserInfo user, string actualName, string newName)
		{
			string requestDescription = string.Format("RenameDeviceAsync(({0}, {1}), {2}, {3})", user.Login, user.Password, actualName, newName);
			_apiLogger.LogRequest(requestDescription);

			string requestContent = JsonConvert.SerializeObject(new DeviceRenameRequest
			{
				ActualName = actualName,
				NewName = newName
			});

			HttpResult result = await _requestService.PostAsync(_apiHost + Uris.DEVICE_RENAME, requestContent, UserHeaders(user));

			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return DeviceStatusCode.InternalError;
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.Accepted:
					return DeviceStatusCode.Ok;
				case HttpStatusCode.BadRequest:
					return DeviceStatusCode.BadRequest;
				case HttpStatusCode.Conflict:
					return DeviceStatusCode.DeviceAlreadyExists;
				case HttpStatusCode.Unauthorized:
					return DeviceStatusCode.InvalidLoginOrPassword;
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return DeviceStatusCode.InternalError;
		}

		public async Task<DeviceStatusCode> ListDevicesAsync(UserInfo user, List<DeviceResponse> resultList)
		{
			string requestDescription = string.Format("ListDevicesAsync(({0}, {1}))", user.Login, user.Password);
			_apiLogger.LogRequest(requestDescription);
			
			HttpResult result = await _requestService.GetAsync(_apiHost + Uris.DEVICE_LIST, UserHeaders(user));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return DeviceStatusCode.InternalError;
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
				{
					RequestResult<List<DeviceResponse>> requestResult = Deserialize<List<DeviceResponse>>(result.Content, requestDescription);
					if (requestResult == null)
					{
						return DeviceStatusCode.InternalError;
					}
					resultList.Clear();
					resultList.AddRange(requestResult.Content);
					return DeviceStatusCode.Ok;
				}
				case HttpStatusCode.Unauthorized:
					return DeviceStatusCode.InvalidLoginOrPassword;
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return DeviceStatusCode.InternalError;
		}

		private RequestResult<T> Deserialize<T>(string data, string requestDescription)
		{
			try
			{
				RequestResult<T> result = JsonConvert.DeserializeObject<RequestResult<T>>(data);

				if (result == null)
				{
					_apiLogger.LogServerError(requestDescription, string.Format("Invalid json content to deserialize to RequestResult<{0}> : {1}", typeof (T), data));
					return null;
				}
				return result;
			}
			catch (Exception)
			{
				//ignore
			}

			_apiLogger.LogServerError(requestDescription, string.Format("Invalid json content to deserialize to RequestResult<{0}> : {1}", typeof(T), data));
			return null;
		}

		#region Headers

		private Dictionary<string, string> DefaultHeaders()
		{
			return new Dictionary<string, string>
			{
				{"Accept", "application/json"}
			};
		}

		private Dictionary<string, string> UserHeaders(UserInfo user)
		{
			Dictionary<string, string> result = DefaultHeaders();
			result.Add(Headers.LOGIN, user.Login);
			result.Add(Headers.PASSWORD, user.Password);
			return result;
		}

		private Dictionary<string, string> DeviceHeaders(UserInfo user, DeviceInfo device)
		{
			Dictionary<string, string> result = UserHeaders(user);
			result.Add(Headers.DEVICE, device.Name);
			return result;
		}

		#endregion

	}
}
