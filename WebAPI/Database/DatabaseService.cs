using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Models;
using Version = WebAPI.Models.Version;

namespace WebAPI.Database
{
	public partial class DatabaseService : IDatabaseService
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

		public void RegisterUser(string login, string email, string password)
		{
			_context.Users.Add(new User
			{
				Login = login,
				Email = email,
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
			return _context.Settings.Where(x => x.DeviceId == device.Id).OrderByDescending(x => x.VersionNumber);
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
				VersionNumber = (lastSettings == null) ? 1 : lastSettings.VersionNumber + 1,
				Date = DateTime.Now
			};

			_context.Settings.Add(newSettings);
			_context.SaveChanges();
			return newSettings;
		}

		#endregion

		#region collection versions
		
		public Version CreateVersion(long userId, long deviceId)
		{
			Version lastVersionData = _context.Versions.Where(x => x.UserId == userId).OrderByDescending(x => x.Number).FirstOrDefault();
			long lastVersion = lastVersionData == null ? 1 : (lastVersionData.Number + 1);
			
			Version version = _context.Versions.Add(new Version
			{
				Date = DateTime.Now,
				Number = lastVersion,
				UserId = userId,
				IsOpen = true,
				DeviceId = deviceId
			});
			_context.SaveChanges();
			return version;
		}

		public Version CloseVersion(long userId, long deviceId, long version)
		{
			Version v = _context.Versions.FirstOrDefault(x => x.UserId == userId && x.Number == version);
			if (v == null || v.DeviceId != deviceId)
			{
				return null;
			}

			v.IsOpen = false;
			_context.SaveChanges();
			return v;
		}

		public bool HasIndiagramVersion(long userId, long version)
		{
			return _context.Versions.FirstOrDefault(x => x.UserId == userId && x.Number == version) != null;
		}

		public bool IsVersionOpen(long userId, long version)
		{
			Version v = _context.Versions.FirstOrDefault(x => x.UserId == userId && x.Number == version);
			return v != null && v.IsOpen;
		}

		public bool CanPushInVersion(long userId, long deviceId, long version)
		{
			Version v = _context.Versions.FirstOrDefault(x => x.UserId == userId && x.Number == version);
			return v != null && v.IsOpen && v.DeviceId == deviceId;
		}

		public List<Version> GetVersions(long userId)
		{
			return _context.Versions.Where(x => x.UserId == userId && !x.IsOpen).OrderByDescending(x => x.Number).ToList();
		}

		public List<Version> GetVersions(long userId, long startVersion)
		{
			return _context.Versions.Where(x => x.UserId == userId && x.Number > startVersion && !x.IsOpen).OrderByDescending(x => x.Number).ToList();
		}

		#endregion
	}
}
