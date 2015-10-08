using System;
using System.Collections.Generic;
using WebAPI.Models;

namespace WebAPI.Database
{
	public interface IDatabaseService : IDisposable
	{
		// user 
		bool UserLoginExists(string login);
		bool UserEmailExists(string email);
		User GetUserByLogin(string login);
		void RegisterUser(string login, string password);
		bool CheckAuthentification(string login, string password);
		
		// device
		void CreateDevice(User user, string name);
		bool HasDevice(User user, string name);
		Device GetDevice(User user, string name);
		bool UpdateDevice(User user, string oldName, string newName);
		IEnumerable<Device> GetDevices(User user);

		// settings
		Settings GetLastSettings(Device device);
		IEnumerable<Settings> GetSettings(Device device);
		Settings GetSettings(Device device, long version);
		Settings CreateSettings(Device device, string settingsData);
	}
}
