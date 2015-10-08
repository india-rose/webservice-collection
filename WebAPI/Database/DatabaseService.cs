﻿using System.Collections.Generic;
using System.Linq;
using WebAPI.Models;

namespace WebAPI.Database
{
	public class DatabaseService : IDatabaseService
	{
		private readonly DatabaseContext _context;

		public DatabaseService()
		{
			_context = new DatabaseContext();
		}
		
		public void Dispose()
		{
			if (_context != null)
			{
				_context.Dispose();
			}
		}

		#region users

		public bool UserLoginExists(string login)
		{
			return _context.Users.Any(x => x.Login == login);
		}

		public bool UserEmailExists(string email)
		{
			return _context.Users.Any(x => x.Email == email);
		}

		public User GetUserByLogin(string login)
		{
			return _context.Users.FirstOrDefault(x => x.Login == login);
		}

		public void RegisterUser(string login, string password)
		{
			_context.Users.Add(new User
			{
				Login = login,
				Password = password.ToUpperInvariant()
			});
			_context.SaveChanges();
		}

		public bool CheckAuthentification(string login, string password)
		{
			User user = GetUserByLogin(login);

			if (user == null)
			{
				return false;
			}

			return user.Password.Equals(password.ToUpperInvariant());
		}

		#endregion

		#region devices

		public bool HasDevice(User user, string name)
		{
			return GetDevice(user, name) != null;
		}

		public void CreateDevice(User user, string name)
		{
			_context.Devices.Add(new Device
			{
				DeviceName = name,
				UserId = user.Id
			});
			_context.SaveChanges();
		}

		public Device GetDevice(User user, string name)
		{
			return GetDevices(user).FirstOrDefault(x => x.DeviceName == name);
		}

		public bool UpdateDevice(User user, string oldName, string newName)
		{
			Device device = GetDevice(user, oldName);

			if (device == null)
			{
				return false;
			}

			device.DeviceName = newName;
			_context.SaveChanges();
			return true;
		}

		public IEnumerable<Device> GetDevices(User user)
		{
			return _context.Devices.Where(x => x.UserId == user.Id);
		}

		#endregion

		#region settings

		public Settings GetLastSettings(Device device)
		{
			return GetSettings(device).OrderByDescending(x => x.VersionNumber).FirstOrDefault();
		}

		public IEnumerable<Settings> GetSettings(Device device)
		{
			return _context.Settings.Where(x => x.DeviceId == device.Id);
		}

		public Settings GetSettings(Device device, long version)
		{
			return GetSettings(device).FirstOrDefault(x => x.VersionNumber == version);
		}

		public Settings CreateSettings(Device device, string settingsData)
		{
			Settings lastSettings = GetLastSettings(device);

			Settings newSettings = new Settings
			{
				DeviceId = device.Id,
				SerializedSettings = settingsData,
				VersionNumber = (lastSettings == null) ? 1 : lastSettings.VersionNumber + 1
			};

			_context.Settings.Add(newSettings);
			_context.SaveChanges();
			return newSettings;
		}

		#endregion

	}
}
