using System;
using System.Collections.Generic;
using WebAPI.Common.Requests;
using WebAPI.Models;
using WebAPI.ProcessModels;
using Version = WebAPI.Models.Version;

namespace WebAPI.Database
{
	public interface IDatabaseService : IDisposable
	{
		// user 
		bool UserLoginExists(string login);
		bool UserEmailExists(string email);
		User GetUserByLogin(string login);
		void RegisterUser(string login, string email, string password);
		bool CheckAuthentification(string login, string password);

		// device
		bool HasDevice(User user, string name);
		void CreateDevice(User user, string name);
		Device GetDevice(User user, string name);
		bool UpdateDevice(User user, string oldName, string newName);
		IEnumerable<Device> GetDevices(User user);

		// settings
		Settings GetLastSettings(Device device);
		IEnumerable<Settings> GetSettings(Device device);
		Settings GetSettings(Device device, long version);
		Settings CreateSettings(Device device, string settingsData);

		// collection
		List<IndiagramForDevice> GetIndiagrams(Device device);
		List<IndiagramForDevice> GetIndiagrams(Device device, long version);
		IndiagramForDevice GetIndiagram(Device device, long id);
		IndiagramForDevice GetIndiagram(Device device, long id, long version);
		IndiagramInfo GetLastIndiagramInfo(long userId, long indiagramId);
		IndiagramInfo GetIndiagramInfo(long userId, long indiagramId, long version);
		IndiagramInfo CreateIndiagramInfo(long userId, long indiagramId, long version);
		IndiagramForDevice CreateIndiagram(long userId, long deviceId, IndiagramRequest request);
		IndiagramForDevice UpdateIndiagram(long userId, long deviceId, IndiagramRequest request);
		void SetIndiagramImage(IndiagramInfo indiagramInfo, string filename, byte[] fileContent);
		void SetIndiagramSound(IndiagramInfo indiagramInfo, string filename, byte[] fileContent);

		// collection versions
		Version CreateVersion(long userId, long deviceId);
		Version CloseVersion(long userId, long deviceId, long version);
		bool HasIndiagramVersion(long userId, long version);
		bool IsVersionOpen(long userId, long version);
		bool CanPushInVersion(long userId, long deviceId, long version);
		List<Version> GetVersions(long userId);
		List<Version> GetVersions(long userId, long startVersion);
		Version GetVersion(long userId, long deviceId, long version);
	}
}
