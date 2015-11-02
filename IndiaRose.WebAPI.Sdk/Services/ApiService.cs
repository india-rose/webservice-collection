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

		public async Task<bool> IsAlive()
		{
			string requestDescription = "IsAlive()";
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + Uris.ALIVE, DefaultHeaders());

			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return false;
			}
			if (result.StatusCode == HttpStatusCode.OK)
			{
				return true;
			}
			_apiLogger.LogServerError(requestDescription, result.Content);
			return false;
		}

		// users
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

		// devices
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

		public async Task<ApiResult<DeviceStatusCode, List<DeviceResponse>>> ListDevicesAsync(UserInfo user)
		{
			string requestDescription = string.Format("ListDevicesAsync(({0}, {1}))", user.Login, user.Password);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + Uris.DEVICE_LIST, UserHeaders(user));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<DeviceStatusCode, List<DeviceResponse>>(DeviceStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<List<DeviceResponse>> requestResult = Deserialize<List<DeviceResponse>>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<DeviceStatusCode, List<DeviceResponse>>(DeviceStatusCode.InternalError, null);
						}
						return ApiResult.From(DeviceStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<DeviceStatusCode, List<DeviceResponse>>(DeviceStatusCode.InvalidLoginOrPassword, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<DeviceStatusCode, List<DeviceResponse>>(DeviceStatusCode.InternalError, null);

		}

		// settings
		public async Task<ApiResult<SettingsStatusCode, SettingsResponse>> GetLastSettingsAsync(UserInfo user, DeviceInfo device)
		{
			string requestDescription = string.Format("GetLastSettingsAsync(({0}, {1}), ({2}))", user.Login, user.Password, device.Name);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + Uris.SETTINGS_LAST, DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<SettingsStatusCode, SettingsResponse>(SettingsStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<SettingsResponse> requestResult = Deserialize<SettingsResponse>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<SettingsStatusCode, SettingsResponse>(SettingsStatusCode.InternalError, null);
						}
						return ApiResult.From(SettingsStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<SettingsStatusCode, SettingsResponse>(SettingsStatusCode.InvalidLoginOrPassword, null);
				case HttpStatusCode.NotFound:
					return ApiResult.From<SettingsStatusCode, SettingsResponse>(SettingsStatusCode.SettingsNotFound, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<SettingsStatusCode, SettingsResponse>(SettingsStatusCode.InternalError, null);
		}

		public async Task<SettingsStatusCode> UpdateSettingsAsync(UserInfo user, DeviceInfo device, string serializedSettingsData)
		{
			string requestDescription = string.Format("UpdateSettingsAsync(({0}, {1}), ({2}), {3})", user.Login, user.Password, device.Name, serializedSettingsData);
			_apiLogger.LogRequest(requestDescription);

			string requestContent = JsonConvert.SerializeObject(new SettingsUpdateRequest
			{
				Data = serializedSettingsData
			});

			HttpResult result = await _requestService.PostAsync(_apiHost + Uris.SETTINGS_UPDATE, requestContent, DeviceHeaders(user, device));

			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return SettingsStatusCode.InternalError;
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					return SettingsStatusCode.Ok;
				case HttpStatusCode.BadRequest:
					return SettingsStatusCode.BadRequest;
				case HttpStatusCode.Unauthorized:
					return SettingsStatusCode.InvalidLoginOrPassword;
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return SettingsStatusCode.InternalError;
		}

		public async Task<ApiResult<SettingsStatusCode, SettingsResponse>> GetVersionSettingsAsync(UserInfo user, DeviceInfo device, long versionNumber)
		{
			string requestDescription = string.Format("GetVersionSettingsAsync(({0}, {1}), ({2}), {3})", user.Login, user.Password, device.Name, versionNumber);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + string.Format(Uris.SETTINGS_GET_VERSION, versionNumber), DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<SettingsStatusCode, SettingsResponse>(SettingsStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<SettingsResponse> requestResult = Deserialize<SettingsResponse>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<SettingsStatusCode, SettingsResponse>(SettingsStatusCode.InternalError, null);
						}
						return ApiResult.From(SettingsStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<SettingsStatusCode, SettingsResponse>(SettingsStatusCode.InvalidLoginOrPassword, null);
				case HttpStatusCode.NotFound:
					return ApiResult.From<SettingsStatusCode, SettingsResponse>(SettingsStatusCode.SettingsNotFound, null);
				case HttpStatusCode.BadRequest:
					return ApiResult.From<SettingsStatusCode, SettingsResponse>(SettingsStatusCode.BadRequest, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<SettingsStatusCode, SettingsResponse>(SettingsStatusCode.InternalError, null);
		}

		public async Task<ApiResult<SettingsStatusCode, List<SettingsResponse>>> GetSettingsListAsync(UserInfo user, DeviceInfo device)
		{
			string requestDescription = string.Format("GetSettingsListAsync(({0}, {1}), ({2}))", user.Login, user.Password, device.Name);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + Uris.SETTINGS_LIST, DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<SettingsStatusCode, List<SettingsResponse>>(SettingsStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<List<SettingsResponse>> requestResult = Deserialize<List<SettingsResponse>>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<SettingsStatusCode, List<SettingsResponse>>(SettingsStatusCode.InternalError, null);
						}
						return ApiResult.From(SettingsStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<SettingsStatusCode, List<SettingsResponse>>(SettingsStatusCode.InvalidLoginOrPassword, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<SettingsStatusCode, List<SettingsResponse>>(SettingsStatusCode.InternalError, null);
		}

		// versions
		public async Task<ApiResult<VersionStatusCode, List<VersionResponse>>> GetVersions(UserInfo user, DeviceInfo device)
		{
			string requestDescription = string.Format("GetVersions(({0}, {1}), ({2}))", user.Login, user.Password, device.Name);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + Uris.VERSIONS_ALL, DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<VersionStatusCode, List<VersionResponse>>(VersionStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<List<VersionResponse>> requestResult = Deserialize<List<VersionResponse>>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<VersionStatusCode, List<VersionResponse>>(VersionStatusCode.InternalError, null);
						}
						return ApiResult.From(VersionStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<VersionStatusCode, List<VersionResponse>>(VersionStatusCode.InvalidLoginOrPassword, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<VersionStatusCode, List<VersionResponse>>(VersionStatusCode.InternalError, null);
		}

		public async Task<ApiResult<VersionStatusCode, List<VersionResponse>>> GetVersions(UserInfo user, DeviceInfo device, long fromVersion)
		{
			string requestDescription = string.Format("GetVersions(({0}, {1}), ({2}), {3})", user.Login, user.Password, device.Name, fromVersion);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + string.Format(Uris.VERSIONS_ALL_FROM, fromVersion), DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<VersionStatusCode, List<VersionResponse>>(VersionStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<List<VersionResponse>> requestResult = Deserialize<List<VersionResponse>>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<VersionStatusCode, List<VersionResponse>>(VersionStatusCode.InternalError, null);
						}
						return ApiResult.From(VersionStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<VersionStatusCode, List<VersionResponse>>(VersionStatusCode.InvalidLoginOrPassword, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<VersionStatusCode, List<VersionResponse>>(VersionStatusCode.InternalError, null);
		}

		public async Task<ApiResult<VersionStatusCode, VersionResponse>> CreateVersion(UserInfo user, DeviceInfo device)
		{
			string requestDescription = string.Format("CreateVersion(({0}, {1}), ({2}))", user.Login, user.Password, device.Name);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.PostAsync(_apiHost + Uris.VERSIONS_CREATE, DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<VersionStatusCode, VersionResponse>(VersionStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<VersionResponse> requestResult = Deserialize<VersionResponse>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<VersionStatusCode, VersionResponse>(VersionStatusCode.InternalError, null);
						}
						return ApiResult.From(VersionStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<VersionStatusCode, VersionResponse>(VersionStatusCode.InvalidLoginOrPassword, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<VersionStatusCode, VersionResponse>(VersionStatusCode.InternalError, null);
		}

		public async Task<ApiResult<VersionStatusCode, VersionResponse>> CloseVersion(UserInfo user, DeviceInfo device, long versionNumber)
		{
			string requestDescription = string.Format("CloseVersion(({0}, {1}), ({2}), {3})", user.Login, user.Password, device.Name, versionNumber);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.PostAsync(_apiHost + string.Format(Uris.VERSIONS_CLOSE, versionNumber), DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<VersionStatusCode, VersionResponse>(VersionStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<VersionResponse> requestResult = Deserialize<VersionResponse>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<VersionStatusCode, VersionResponse>(VersionStatusCode.InternalError, null);
						}
						return ApiResult.From(VersionStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<VersionStatusCode, VersionResponse>(VersionStatusCode.InvalidLoginOrPassword, null);
				case HttpStatusCode.BadRequest:
					return ApiResult.From<VersionStatusCode, VersionResponse>(VersionStatusCode.BadRequest, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<VersionStatusCode, VersionResponse>(VersionStatusCode.InternalError, null);
		}

		// collections
		public async Task<ApiResult<IndiagramStatusCode, IndiagramResponse>> UpdateIndiagram(UserInfo user, DeviceInfo device, IndiagramRequest indiagram)
		{
			string requestDescription = string.Format("UpdateIndiagram(({0}, {1}), ({2}), {3})", user.Login, user.Password, device.Name, indiagram);
			_apiLogger.LogRequest(requestDescription);

			string requestContent = JsonConvert.SerializeObject(indiagram);

			HttpResult result = await _requestService.PostAsync(_apiHost + Uris.INDIAGRAM_UPDATE, requestContent, DeviceHeaders(user, device));

			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return new ApiResult<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
				{
					RequestResult<IndiagramResponse> requestResult = Deserialize<IndiagramResponse>(result.Content, requestDescription);
					if (requestResult == null)
					{
						return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InternalError, null);
					}
					return ApiResult.From(IndiagramStatusCode.Ok, requestResult.Content);
				}
				case HttpStatusCode.BadRequest:
					return new ApiResult<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.BadRequest, null);
				case HttpStatusCode.Unauthorized:
					return new ApiResult<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InvalidLoginOrPassword, null);
				case HttpStatusCode.NotFound:
					return new ApiResult<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.IndiagramNotFound, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return new ApiResult<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InternalError, null);
		}

		public async Task<ApiResult<IndiagramStatusCode, List<MappedIndiagramResponse>>> UpdateIndiagrams(UserInfo user, DeviceInfo device, List<IndiagramRequest> indiagrams)
		{
			string requestDescription = string.Format("UpdateIndiagrams(({0}, {1}), ({2}), (Count = {3}))", user.Login, user.Password, device.Name, indiagrams.Count);
			_apiLogger.LogRequest(requestDescription);

			string requestContent = JsonConvert.SerializeObject(indiagrams);

			HttpResult result = await _requestService.PostAsync(_apiHost + Uris.INDIAGRAM_MULTI_UPDATE, requestContent, DeviceHeaders(user, device));

			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return new ApiResult<IndiagramStatusCode, List<MappedIndiagramResponse>>(IndiagramStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<List<MappedIndiagramResponse>> requestResult = Deserialize<List<MappedIndiagramResponse>>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<IndiagramStatusCode, List<MappedIndiagramResponse>>(IndiagramStatusCode.InternalError, null);
						}
						return ApiResult.From(IndiagramStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.BadRequest:
					return new ApiResult<IndiagramStatusCode, List<MappedIndiagramResponse>>(IndiagramStatusCode.BadRequest, null);
				case HttpStatusCode.Unauthorized:
					return new ApiResult<IndiagramStatusCode, List<MappedIndiagramResponse>>(IndiagramStatusCode.InvalidLoginOrPassword, null);
				case HttpStatusCode.NotFound:
					return new ApiResult<IndiagramStatusCode, List<MappedIndiagramResponse>>(IndiagramStatusCode.IndiagramNotFound, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return new ApiResult<IndiagramStatusCode, List<MappedIndiagramResponse>>(IndiagramStatusCode.InternalError, null);
		}

		public async Task<IndiagramStatusCode> UploadImage(UserInfo user, DeviceInfo device, long indiagramId, long versionNumber, string filename, byte[] content)
		{
			string requestDescription = string.Format("UploadImage(({0}, {1}), ({2}), {3}, {4}, {5})", user.Login, user.Password, device.Name, indiagramId, versionNumber, filename);
			_apiLogger.LogRequest(requestDescription);

			
			string requestContent = JsonConvert.SerializeObject(new FileUploadRequest
			{
				Filename = filename,
				Content = content
			});

			HttpResult result = await _requestService.PostAsync(_apiHost + string.Format(Uris.IMAGE_UPLOAD, indiagramId, versionNumber), requestContent, DeviceHeaders(user, device));

			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return IndiagramStatusCode.InternalError;
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					return IndiagramStatusCode.Ok;
				case HttpStatusCode.BadRequest:
					return IndiagramStatusCode.BadRequest;
				case HttpStatusCode.Unauthorized:
					return IndiagramStatusCode.InvalidLoginOrPassword;
				case HttpStatusCode.Forbidden:
					return IndiagramStatusCode.Forbidden;
				case HttpStatusCode.NotFound:
					return IndiagramStatusCode.IndiagramNotFound;
				case HttpStatusCode.Conflict:
					return IndiagramStatusCode.FileAlreadyExists;
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return IndiagramStatusCode.InternalError;
		}

		public async Task<IndiagramStatusCode> UploadSound(UserInfo user, DeviceInfo device, long indiagramId, long versionNumber, string filename, byte[] content)
		{
			string requestDescription = string.Format("UploadSound(({0}, {1}), ({2}), {3}, {4}, {5})", user.Login, user.Password, device.Name, indiagramId, versionNumber, filename);
			_apiLogger.LogRequest(requestDescription);


			string requestContent = JsonConvert.SerializeObject(new FileUploadRequest
			{
				Filename = filename,
				Content = content
			});

			HttpResult result = await _requestService.PostAsync(_apiHost + string.Format(Uris.SOUND_UPLOAD, indiagramId, versionNumber), requestContent, DeviceHeaders(user, device));

			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return IndiagramStatusCode.InternalError;
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					return IndiagramStatusCode.Ok;
				case HttpStatusCode.BadRequest:
					return IndiagramStatusCode.BadRequest;
				case HttpStatusCode.Unauthorized:
					return IndiagramStatusCode.InvalidLoginOrPassword;
				case HttpStatusCode.Forbidden:
					return IndiagramStatusCode.Forbidden;
				case HttpStatusCode.NotFound:
					return IndiagramStatusCode.IndiagramNotFound;
				case HttpStatusCode.Conflict:
					return IndiagramStatusCode.FileAlreadyExists;
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return IndiagramStatusCode.InternalError;
		}

		public async Task<ApiResult<IndiagramStatusCode, List<IndiagramResponse>>> GetAllCollection(UserInfo user, DeviceInfo device)
		{
			string requestDescription = string.Format("GetAllCollection(({0}, {1}), ({2}))", user.Login, user.Password, device.Name);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + Uris.COLLECTION_ALL, DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<IndiagramStatusCode, List<IndiagramResponse>>(IndiagramStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<List<IndiagramResponse>> requestResult = Deserialize<List<IndiagramResponse>>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<IndiagramStatusCode, List<IndiagramResponse>>(IndiagramStatusCode.InternalError, null);
						}
						return ApiResult.From(IndiagramStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<IndiagramStatusCode, List<IndiagramResponse>>(IndiagramStatusCode.InvalidLoginOrPassword, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<IndiagramStatusCode, List<IndiagramResponse>>(IndiagramStatusCode.InternalError, null);
		}

		public async Task<ApiResult<IndiagramStatusCode, List<IndiagramResponse>>> GetAllCollection(UserInfo user, DeviceInfo device, long version)
		{
			string requestDescription = string.Format("GetAllCollection(({0}, {1}), ({2}), {3})", user.Login, user.Password, device.Name, version);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + string.Format(Uris.COLLECTION_ALL_IN_VERSION, version), DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<IndiagramStatusCode, List<IndiagramResponse>>(IndiagramStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<List<IndiagramResponse>> requestResult = Deserialize<List<IndiagramResponse>>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<IndiagramStatusCode, List<IndiagramResponse>>(IndiagramStatusCode.InternalError, null);
						}
						return ApiResult.From(IndiagramStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<IndiagramStatusCode, List<IndiagramResponse>>(IndiagramStatusCode.InvalidLoginOrPassword, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<IndiagramStatusCode, List<IndiagramResponse>>(IndiagramStatusCode.InternalError, null);
		}

		public async Task<ApiResult<IndiagramStatusCode, IndiagramResponse>> GetIndiagram(UserInfo user, DeviceInfo device, long indiagramId)
		{
			string requestDescription = string.Format("GetIndiagram(({0}, {1}), ({2}), {3})", user.Login, user.Password, device.Name, indiagramId);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + string.Format(Uris.INDIAGRAM_GET, indiagramId), DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<IndiagramResponse> requestResult = Deserialize<IndiagramResponse>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InternalError, null);
						}
						return ApiResult.From(IndiagramStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InvalidLoginOrPassword, null);
				case HttpStatusCode.NotFound:
					return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.IndiagramNotFound, null);
				case HttpStatusCode.BadRequest:
					return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.BadRequest, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InternalError, null);
		}

		public async Task<ApiResult<IndiagramStatusCode, IndiagramResponse>> GetIndiagram(UserInfo user, DeviceInfo device, long indiagramId, long version)
		{
			string requestDescription = string.Format("GetIndiagram(({0}, {1}), ({2}), {3}, {4})", user.Login, user.Password, device.Name, indiagramId, version);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + string.Format(Uris.INDIAGRAM_GET_IN_VERSION, indiagramId, version), DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<IndiagramResponse> requestResult = Deserialize<IndiagramResponse>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InternalError, null);
						}
						return ApiResult.From(IndiagramStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InvalidLoginOrPassword, null);
				case HttpStatusCode.NotFound:
					return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.IndiagramNotFound, null);
				case HttpStatusCode.BadRequest:
					return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.BadRequest, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<IndiagramStatusCode, IndiagramResponse>(IndiagramStatusCode.InternalError, null);
		}

		public async Task<ApiResult<IndiagramStatusCode, FileDownloadResponse>> GetImage(UserInfo user, DeviceInfo device, long indiagramId)
		{
			string requestDescription = string.Format("GetImage(({0}, {1}), ({2}), {3})", user.Login, user.Password, device.Name, indiagramId);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + string.Format(Uris.IMAGE_GET, indiagramId), DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<FileDownloadResponse> requestResult = Deserialize<FileDownloadResponse>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
						}
						return ApiResult.From(IndiagramStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InvalidLoginOrPassword, null);
				case HttpStatusCode.NotFound:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.IndiagramNotFound, null);
				case HttpStatusCode.BadRequest:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.BadRequest, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
		}

		public async Task<ApiResult<IndiagramStatusCode, FileDownloadResponse>> GetImage(UserInfo user, DeviceInfo device, long indiagramId, long version)
		{
			string requestDescription = string.Format("GetImage(({0}, {1}), ({2}), {3}, {4})", user.Login, user.Password, device.Name, indiagramId, version);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + string.Format(Uris.IMAGE_GET_IN_VERSION, indiagramId, version), DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<FileDownloadResponse> requestResult = Deserialize<FileDownloadResponse>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
						}
						return ApiResult.From(IndiagramStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InvalidLoginOrPassword, null);
				case HttpStatusCode.NotFound:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.IndiagramNotFound, null);
				case HttpStatusCode.BadRequest:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.BadRequest, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
		}

		public async Task<ApiResult<IndiagramStatusCode, FileDownloadResponse>> GetSound(UserInfo user, DeviceInfo device, long indiagramId)
		{
			string requestDescription = string.Format("GetSound(({0}, {1}), ({2}), {3})", user.Login, user.Password, device.Name, indiagramId);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + string.Format(Uris.SOUND_GET, indiagramId), DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<FileDownloadResponse> requestResult = Deserialize<FileDownloadResponse>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
						}
						return ApiResult.From(IndiagramStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InvalidLoginOrPassword, null);
				case HttpStatusCode.NotFound:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.IndiagramNotFound, null);
				case HttpStatusCode.BadRequest:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.BadRequest, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
		}

		public async Task<ApiResult<IndiagramStatusCode, FileDownloadResponse>> GetSound(UserInfo user, DeviceInfo device, long indiagramId, long version)
		{
			string requestDescription = string.Format("GetSound(({0}, {1}), ({2}), {3}, {4})", user.Login, user.Password, device.Name, indiagramId, version);
			_apiLogger.LogRequest(requestDescription);

			HttpResult result = await _requestService.GetAsync(_apiHost + string.Format(Uris.SOUND_GET_IN_VERSION, indiagramId, version), DeviceHeaders(user, device));
			if (result.InnerException != null)
			{
				_apiLogger.LogError(requestDescription, result.InnerException);
				return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
			}
			switch (result.StatusCode)
			{
				case HttpStatusCode.OK:
					{
						RequestResult<FileDownloadResponse> requestResult = Deserialize<FileDownloadResponse>(result.Content, requestDescription);
						if (requestResult == null)
						{
							return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
						}
						return ApiResult.From(IndiagramStatusCode.Ok, requestResult.Content);
					}
				case HttpStatusCode.Unauthorized:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InvalidLoginOrPassword, null);
				case HttpStatusCode.NotFound:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.IndiagramNotFound, null);
				case HttpStatusCode.BadRequest:
					return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.BadRequest, null);
			}

			_apiLogger.LogServerError(requestDescription, result.Content);
			return ApiResult.From<IndiagramStatusCode, FileDownloadResponse>(IndiagramStatusCode.InternalError, null);
		}

		#region Private methods

		private RequestResult<T> Deserialize<T>(string data, string requestDescription)
		{
			try
			{
				RequestResult<T> result = JsonConvert.DeserializeObject<RequestResult<T>>(data);

				if (result == null)
				{
					_apiLogger.LogServerError(requestDescription, string.Format("Invalid json content to deserialize to RequestResult<{0}> : {1}", typeof(T), data));
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
