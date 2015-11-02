using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using WebAPI.Common.Requests;
using WebAPI.Models;
using WebAPI.ProcessModels;

namespace WebAPI.Database
{
	public partial class DatabaseService
	{
		#region Get IndiagramForDevice

		public List<IndiagramForDevice> GetIndiagrams(Device device)
		{
			return GetIndiagramsUser(device.UserId).Select(x => ToIndiagramForDevice(device, x, false)).OrderBy(x => x.Position).ToList();
		}

		public List<IndiagramForDevice> GetIndiagrams(Device device, long version)
		{
			return GetIndiagramsUser(device.UserId).Select(x => ToIndiagramForDevice(device, x, version)).OrderBy(x => x.Position).ToList();
		}

		public IndiagramForDevice GetIndiagram(Device device, long id)
		{
			Indiagram indiagram = GetIndiagramUser(device.UserId, id);
			return indiagram == null ? null : ToIndiagramForDevice(device, indiagram, false);
		}

		public IndiagramForDevice GetIndiagram(Device device, long id, long version)
		{
			Indiagram indiagram = GetIndiagramUser(device.UserId, id);
			return indiagram == null ? null : ToIndiagramForDevice(device, indiagram, version);
		}

		#endregion

		#region Get IndiagramInfo

		public IndiagramInfo GetLastIndiagramInfo(long userId, long indiagramId)
		{
			Indiagram indiagram = _context.Indiagrams.FirstOrDefault(x => x.UserId == userId && x.Id == indiagramId);
			if (indiagram == null)
			{
				return null;
			}

			return indiagram.LastIndiagramInfo;
		}

		public IndiagramInfo GetIndiagramInfo(long userId, long indiagramId, long version)
		{
			Indiagram indiagram = _context.Indiagrams.FirstOrDefault(x => x.UserId == userId && x.Id == indiagramId);
			if (indiagram == null)
			{
				return null;
			}

			return indiagram.Infos.FirstOrDefault(x => x.Version == version);
		}

		#endregion

		public IndiagramInfo CreateIndiagramInfo(long userId, long indiagramId, long version)
		{
			Indiagram indiagram = _context.Indiagrams.FirstOrDefault(x => x.UserId == userId && x.Id == indiagramId);
			if (indiagram == null)
			{
				return null;
			}

			return CreateIndiagramInfo(indiagram, version);
		}

		private IndiagramInfo CreateIndiagramInfo(Indiagram indiagram, long version)
		{
			IndiagramInfo info = indiagram.Infos.OrderByDescending(x => x.Version).FirstOrDefault(x => x.Version <= version);
			if (info == null)
			{
				// no version available
				return null;
			}

			if (info.Version == version)
			{
				return info;
			}

			// create IndiagramInfo based on last version available
			return CreateIndiagramInfo(indiagram, info, version);
		}

		private IndiagramInfo CreateIndiagramInfo(Indiagram indiagram, IndiagramInfo old, long version)
		{
			IndiagramInfo info = _context.Set<IndiagramInfo>().Add(new IndiagramInfo
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
			});
			DbSet<IndiagramState> stateSet = _context.Set<IndiagramState>();

			info.States = old.States.Select(x => stateSet.Add(
				new IndiagramState
				{
					DeviceId = x.DeviceId,
					IndiagramInfoId = info.Id,
					IsEnabled = x.IsEnabled
				})).ToList();

			if (indiagram.LastIndiagramInfoId == null || indiagram.LastIndiagramInfo.Version < version)
			{
				indiagram.LastIndiagramInfoId = info.Id;
			}

			_context.SaveChanges();
			return info;
		}

		public IndiagramForDevice CreateIndiagram(long userId, long deviceId, IndiagramRequest request)
		{
			Indiagram indiagram = _context.Indiagrams.Add(new Indiagram
			{
				UserId = userId
			});

			IndiagramInfo info = _context.Set<IndiagramInfo>().Add(new IndiagramInfo
			{
				IndiagramId = indiagram.Id,
				Version = request.Version,
				ParentId = request.ParentId,
				Position = request.Position,
				Text = request.Text,
				IsCategory = request.IsCategory
			});
			_context.SaveChanges();

			indiagram.LastIndiagramInfoId = info.Id;

			IndiagramState state = _context.Set<IndiagramState>().Add(new IndiagramState
			{
				DeviceId = deviceId,
				IndiagramInfoId = info.Id,
				IsEnabled = request.IsEnabled
			});


			_context.SaveChanges();
			return ToIndiagramForDevice(indiagram, info, state);
		}

		public IndiagramForDevice UpdateIndiagram(long userId, long deviceId, IndiagramRequest request)
		{
			Indiagram indiagram = GetIndiagramUser(userId, request.Id);

			if (indiagram == null)
			{
				return null;
			}

			IndiagramInfo info = CreateIndiagramInfo(indiagram, request.Version);

			info.ParentId = request.ParentId;
			info.Position = request.Position;
			info.Text = request.Text;
			info.IsCategory = request.IsCategory;

			IndiagramState state = info.States.FirstOrDefault(x => x.DeviceId == deviceId);
			if (state != null)
			{
				state.IsEnabled = request.IsEnabled;
			}
			else
			{
				_context.Set<IndiagramState>().Add(new IndiagramState
				{
					DeviceId = deviceId,
					IndiagramInfoId = info.Id,
					IsEnabled = request.IsEnabled
				});
			}
			_context.SaveChanges();
			return ToIndiagramForDevice(indiagram, info, state);
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

		#region private methods

		private IEnumerable<Indiagram> GetIndiagramsUser(long userId)
		{
			return _context.Indiagrams.Where(x => x.UserId == userId);
		}

		private Indiagram GetIndiagramUser(long userId, long indiagramId)
		{
			return _context.Indiagrams.FirstOrDefault(x => x.UserId == userId && x.Id == indiagramId);
		}

		#endregion 

		#region converter methods

		private IndiagramForDevice ToIndiagramForDevice(Device device, Indiagram indiagram, bool allowOpenedVersion)
		{
			IndiagramInfo info = indiagram.LastIndiagramInfo;
			if (info == null)
			{
				return null;
			}

			if (!allowOpenedVersion && IsVersionOpen(device.UserId, info.Version))
			{
				info = indiagram.Infos.OrderByDescending(item => item.Version).FirstOrDefault(item => !IsVersionOpen(device.UserId, item.Version));

				if (info == null)
				{
					return null;
				}
			}

			return ToIndiagramForDevice(device, indiagram, info);
		}

		private IndiagramForDevice ToIndiagramForDevice(Device device, Indiagram indiagram, long version)
		{
			IndiagramInfo info = indiagram.Infos.OrderByDescending(item => item.Version).FirstOrDefault(item => item.Version <= version);
			if (info == null)
			{
				return null;
			}

			return ToIndiagramForDevice(device, indiagram, info);
		}

		private IndiagramForDevice ToIndiagramForDevice(Device device, Indiagram indiagram, IndiagramInfo info)
		{
			IndiagramState state = info.States.FirstOrDefault(s => s.DeviceId == device.Id);

			return ToIndiagramForDevice(indiagram, info, state);
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
				IsEnabled = state == null || state.IsEnabled,
				ImageFile = info.ImagePath,
				SoundFile = info.SoundPath
			};
		}

		#endregion

		#region tools methods

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
	}
}
