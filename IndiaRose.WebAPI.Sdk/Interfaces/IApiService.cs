using System.Collections.Generic;
using System.Threading.Tasks;
using IndiaRose.WebAPI.Sdk.Models;
using IndiaRose.WebAPI.Sdk.Results;
using WebAPI.Common.Requests;
using WebAPI.Common.Responses;

namespace IndiaRose.WebAPI.Sdk.Interfaces
{
	public interface IApiService
	{
		/// <summary>
		/// Check if webservice is alive
		/// </summary>
		/// <returns>True if success, false otherwise</returns>
		Task<bool> IsAlive();

		/// <summary>
		/// Register a user on the api.
		/// </summary>
		/// <param name="login">user login</param>
		/// <param name="email">user email</param>
		/// <param name="password">user password</param>
		/// <returns>status code</returns>
		Task<UserStatusCode> RegisterUserAsync(string login, string email, string password);

		/// <summary>
		/// Check if user login information are corrects.
		/// </summary>
		/// <param name="login">user login</param>
		/// <param name="password">user password (sha 256)</param>
		/// <returns>status code</returns>
		Task<UserStatusCode> LoginUserAsync(string login, string password);

		/// <summary>
		/// Create device with specified name
		/// </summary>
		/// <param name="user">user information</param>
		/// <param name="deviceName">name of the new device</param>
		/// <returns>status code</returns>
		Task<DeviceStatusCode> CreateDeviceAsync(UserInfo user, string deviceName);

		/// <summary>
		/// Rename device
		/// </summary>
		/// <param name="user">user information</param>
		/// <param name="actualName">actual name of the device</param>
		/// <param name="newName">new name of the device</param>
		/// <returns>status code</returns>
		Task<DeviceStatusCode> RenameDeviceAsync(UserInfo user, string actualName, string newName);

		/// <summary>
		/// List all devices for user
		/// </summary>
		/// <param name="user">user information</param>
		/// <returns>status code + content result</returns>
		Task<ApiResult<DeviceStatusCode, List<DeviceResponse>>> ListDevicesAsync(UserInfo user);

		/// <summary>
		/// Get last version of settings for user/device pair
		/// </summary>
		/// <param name="user">user information</param>
		/// <param name="device">device information</param>
		/// <returns>status code + content result</returns>
		Task<ApiResult<SettingsStatusCode, SettingsResponse>> GetLastSettingsAsync(UserInfo user, DeviceInfo device);

		/// <summary>
		/// Create new version of settings for user/device pair
		/// </summary>
		/// <param name="user">user information</param>
		/// <param name="device">device information</param>
		/// <param name="serializedSettingsData">settings data</param>
		/// <returns>status code</returns>
		Task<SettingsStatusCode> UpdateSettingsAsync(UserInfo user, DeviceInfo device, string serializedSettingsData);

		/// <summary>
		/// Get specific version of settings for user/device pair
		/// </summary>
		/// <param name="user">user information</param>
		/// <param name="device">device information</param>
		/// <param name="versionNumber">version number</param>
		/// <returns>status code + content result</returns>
		Task<ApiResult<SettingsStatusCode, SettingsResponse>> GetVersionSettingsAsync(UserInfo user, DeviceInfo device, long versionNumber);

		/// <summary>
		/// Get lists of settings for user/device pair
		/// </summary>
		/// <param name="user">user information</param>
		/// <param name="device">device information</param>
		/// <returns>status code + content result</returns>
		Task<ApiResult<SettingsStatusCode, List<SettingsResponse>>> GetSettingsListAsync(UserInfo user, DeviceInfo device);


		Task<ApiResult<VersionStatusCode, List<VersionResponse>>> GetVersions(UserInfo user, DeviceInfo device);

		Task<ApiResult<VersionStatusCode, List<VersionResponse>>> GetVersions(UserInfo user, DeviceInfo device, long fromVersion);

		Task<ApiResult<VersionStatusCode, VersionResponse>> CreateVersion(UserInfo user, DeviceInfo device);

		Task<ApiResult<VersionStatusCode, VersionResponse>> CloseVersion(UserInfo user, DeviceInfo device, long versionNumber);

		//== collections ==
		Task<ApiResult<IndiagramStatusCode, IndiagramResponse>> UpdateIndiagram(UserInfo user, DeviceInfo device, IndiagramRequest indiagram);
		Task<ApiResult<IndiagramStatusCode, List<MappedIndiagramResponse>>> UpdateIndiagrams(UserInfo user, DeviceInfo device, List<IndiagramRequest> indiagrams);
		Task<IndiagramStatusCode> UploadImage(UserInfo user, DeviceInfo device, long indiagramId, long versionNumber, string filename, byte[] content);
		Task<IndiagramStatusCode> UploadSound(UserInfo user, DeviceInfo device, long indiagramId, long versionNumber, string filename, byte[] content);

		Task<ApiResult<IndiagramStatusCode, IndiagramResponse>> GetIndiagram(UserInfo user, DeviceInfo device, long indiagramId, long version);
		Task<ApiResult<IndiagramStatusCode, IndiagramResponse>> GetIndiagram(UserInfo user, DeviceInfo device, long indiagramId);
		Task<ApiResult<IndiagramStatusCode, List<IndiagramResponse>>> GetAllCollection(UserInfo user, DeviceInfo device, long version);
		Task<ApiResult<IndiagramStatusCode, List<IndiagramResponse>>> GetAllCollection(UserInfo user, DeviceInfo device);

		Task<ApiResult<IndiagramStatusCode, FileDownloadResponse>> GetImage(UserInfo user, DeviceInfo device, long indiagramId);
		Task<ApiResult<IndiagramStatusCode, FileDownloadResponse>> GetImage(UserInfo user, DeviceInfo device, long indiagramId, long version);
		Task<ApiResult<IndiagramStatusCode, FileDownloadResponse>> GetSound(UserInfo user, DeviceInfo device, long indiagramId);
		Task<ApiResult<IndiagramStatusCode, FileDownloadResponse>> GetSound(UserInfo user, DeviceInfo device, long indiagramId, long version);

	}
}
