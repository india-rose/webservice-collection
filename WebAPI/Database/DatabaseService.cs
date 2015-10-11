using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using WebAPI.Common.Requests;
using WebAPI.Models;
using WebAPI.ProcessModels;
using Version = WebAPI.Models.Version;

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

		private IEnumerable<Indiagram> GetIndiagramsUser(long userId)
		{
			return _context.Indiagrams.Where(x => x.UserId == userId);
		}

		private Indiagram GetIndiagramUser(long userId, long indiagramId)
		{
			return _context.Indiagrams.FirstOrDefault(x => x.UserId == userId && x.Id == indiagramId);
		}

		private IndiagramForDevice CreateIndiagramForDevice(Device device, Indiagram indiagram)
		{
			IndiagramInfo info = indiagram.LastIndiagramInfo;
			IndiagramState state = info.States.FirstOrDefault(s => s.DeviceId == device.Id);

			return ToIndiagramForDevice(indiagram, info, state);
		}

		private IndiagramForDevice CreateIndiagramForDevice(Device device, Indiagram indiagram, long version)
		{
			IndiagramInfo info = indiagram.Infos.OrderByDescending(item => item.Version).First(item => item.Version <= version);
			IndiagramState state = info.States.FirstOrDefault(s => s.DeviceId == device.Id);

			return ToIndiagramForDevice(indiagram, info, state);
		}

		public List<IndiagramForDevice> GetIndiagrams(Device device)
		{
			return GetIndiagramsUser(device.UserId).Select(x => CreateIndiagramForDevice(device, x)).OrderBy(x => x.Position).ToList();
		}

		public List<IndiagramForDevice> GetIndiagrams(Device device, long version)
		{
			return GetIndiagramsUser(device.UserId).Select(x => CreateIndiagramForDevice(device, x, version)).OrderBy(x => x.Position).ToList();
		}

		public IndiagramForDevice GetIndiagram(Device device, long id)
		{
			Indiagram indiagram = GetIndiagramUser(device.UserId, id);
			return indiagram == null ? null : CreateIndiagramForDevice(device, indiagram);
		}

		public IndiagramForDevice GetIndiagram(Device device, long id, long version)
		{
			Indiagram indiagram = GetIndiagramUser(device.UserId, id);
			return indiagram == null ? null : CreateIndiagramForDevice(device, indiagram, version);
		}

		private IndiagramForDevice ToIndiagramForDevice(Indiagram indiagram, IndiagramInfo info, IndiagramState state)
		{
			return new IndiagramForDevice
			{
				Id = indiagram.Id,
				Version = info.Version,
				ImageHash = info.ImageHash,
				IsCategory = info.IsCategory,
				ParentId = info.ParentId,
				SoundHash = info.SoundHash,
				Text = info.Text,
				Position = info.Position,
				IsEnabled = state == null || state.IsEnabled
			};
		}

		public Indiagram CreateIndiagram(long userId, long deviceId, IndiagramRequest indiagram)
		{
			Indiagram result = _context.Indiagrams.Add(new Indiagram
			{
				UserId = userId,
			});

			IndiagramInfo info = _context.Set<IndiagramInfo>().Add(new IndiagramInfo
			{
				//TODO : check if id is set when add is done are we need to call saveChanges
				IndiagramId = result.Id,
				IsCategory = indiagram.IsCategory,
				ParentId = indiagram.ParentId,
				Position = indiagram.Position,
				Text = indiagram.Text,
			});

			result.Infos = new List<IndiagramInfo>{ info };
			result.LastIndiagramInfo = info;
			result.LastIndiagramInfoId = info.Id;

			IndiagramState state = _context.Set<IndiagramState>().Add(new IndiagramState
			{
				DeviceId = deviceId,
				IndiagramInfoId = info.Id,
				IsEnabled = indiagram.IsEnabled
			});

			info.States = new List<IndiagramState> {state};

			_context.SaveChanges();
			return result;
		}

		public IndiagramInfo GetOrCreateIndiagramInfo(long userId, long indiagramId, long version)
		{
			Indiagram indiagram = _context.Indiagrams.FirstOrDefault(x => x.UserId == userId && x.Id == indiagramId);
			if (indiagram == null)
			{
				return null;
			}

			IndiagramInfo info = indiagram.LastIndiagramInfo;
			if (info == null)
			{
				// create the IndiagramInfo object for version based on the last one
				info = new IndiagramInfo
				{
					IndiagramId = indiagram.Id,
					Version = version,
					ParentId = -1,
				};

				info = _context.Set<IndiagramInfo>().Add(info);
				_context.SaveChanges();
			}
			else if (info.Version > version)
			{
				return null; // can not modify old versions
			}
			else if (info.Version < version)
			{
				// create the IndiagramInfo object for version based on the last one
				IndiagramInfo old = info;
				info = new IndiagramInfo
				{
					IndiagramId = indiagram.Id,
					Version = version,
					ParentId = old.ParentId,
					Position = old.Position,
					Text = old.Text,
					SoundPath = old.SoundPath,
					SoundHash = old.SoundHash,
					ImagePath = old.ImagePath,
					ImageHash = old.ImageHash,
					IsCategory = old.IsCategory,
				};
				
				info = _context.Set<IndiagramInfo>().Add(info);
				DbSet<IndiagramState> stateSet = _context.Set<IndiagramState>();

				info.States = old.States.Select(x => stateSet.Add(
					new IndiagramState
					{
						DeviceId = x.DeviceId,
						IndiagramInfoId = info.Id,
						IsEnabled = x.IsEnabled
					})).ToList();

				_context.SaveChanges();
			}

			return info;
		}

		public void SetIndiagramImage(IndiagramInfo indiagramInfo, string filename, byte[] fileContent)
		{
			indiagramInfo.ImagePath = filename;
			indiagramInfo.ImageHash = ComputeFileHash(fileContent);
			_context.SaveChanges();
		}

		public void SetIndiagramSound(IndiagramInfo indiagramInfo, string filename, byte[] fileContent)
		{
			indiagramInfo.SoundPath = filename;
			indiagramInfo.SoundHash = ComputeFileHash(fileContent);
			_context.SaveChanges();
		}

		private string ComputeFileHash(byte[] content)
		{
			using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
			{
				byte[] hash = sha1.ComputeHash(content);
				string hex = BitConverter.ToString(hash);
				return hex.Replace("-", "").ToUpperInvariant();
			}
		}

		#endregion

		#region collection versions

		public Version CreateVersion(long userId)
		{
			long lastVersion = 1;
			if (_context.Versions.Any(x => x.UserId == userId))
			{
				lastVersion = _context.Versions.Where(x => x.UserId == userId).Max(x => x.Number);
			}

			Version version = _context.Versions.Add(new Version
			{
				Date = DateTime.Now,
				Number = lastVersion,
				UserId = userId
			});
			_context.SaveChanges();
			return version;
		}

		public bool HasIndiagramVersion(long userId, long version)
		{
			return _context.Versions.FirstOrDefault(x => x.UserId == userId && x.Number == version) != null;
		}

		public List<Version> GetVersions(long userId)
		{
			return _context.Versions.Where(x => x.UserId == userId).OrderByDescending(x => x.Number).ToList();
		}

		public List<Version> GetVersions(long userId, long startVersion)
		{
			return _context.Versions.Where(x => x.UserId == userId && x.Number > startVersion).OrderByDescending(x => x.Number).ToList();
		}

		#endregion
	}
}
