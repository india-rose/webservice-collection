using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Models;
using WebAPI.ProcessModels;

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
				VersionNumber = (lastSettings == null) ? 1 : lastSettings.VersionNumber + 1,
				Date = DateTime.Now
			};

			_context.Settings.Add(newSettings);
			_context.SaveChanges();
			return newSettings;
		}

		#endregion

		#region collection

		private List<Indiagram> GetIndiagramsUser(long userId)
		{
			return _context.Indiagrams.Where(x => x.UserId == userId).ToList();
		}

		public List<IndiagramForDevice> GetIndiagrams(Device device)
		{
			List<Indiagram> collections = GetIndiagramsUser(device.UserId);

			return collections.Select(x =>
			{
				IndiagramInfo info = x.LastIndiagramInfo;
				IndiagramState state = x.States.Where(s => s.DeviceId == device.Id).OrderByDescending(s => s.Version).FirstOrDefault();

				return ToIndiagramForDevice(x, info, state);
			}).OrderBy(x => x.Position).ToList();
		}

		public List<IndiagramForDevice> GetIndiagrams(Device device, long version)
		{
			List<Indiagram> collections = GetIndiagramsUser(device.UserId);

			return collections.Select(x =>
			{
				IndiagramInfo info = x.Infos.OrderByDescending(item => item.Version).First(item => item.Version <= version);
				IndiagramState state = x.States.Where(s => s.DeviceId == device.Id).OrderByDescending(s => s.Version).FirstOrDefault(s => s.Version <= version);

				return ToIndiagramForDevice(x, info, state);
			}).OrderBy(x => x.Position).ToList();
		}

		private IndiagramForDevice ToIndiagramForDevice(Indiagram indiagram, IndiagramInfo info, IndiagramState state)
		{
			return new IndiagramForDevice
			{
				Id = indiagram.Id,
				Version = info.Version,
				ImagePath = info.ImagePath,
				IsCategory = info.IsCategory,
				ParentId = info.ParentId,
				SoundPath = info.SoundPath,
				Text = info.Text,
				Position = info.Position,
				IsEnabled = state == null || state.IsEnabled
			};
		}

		public bool HasIndiagramVersion(long userId, long version)
		{
			return _context.Versions.FirstOrDefault(x => x.UserId == userId) != null;
		}

		#endregion

	}
}
