﻿using System;
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

		// collection
		List<IndiagramForDevice> GetIndiagrams(Device device);
		List<IndiagramForDevice> GetIndiagrams(Device device, long version);
		IndiagramForDevice GetIndiagram(Device device, long id);
		IndiagramForDevice GetIndiagram(Device device, long id, long version);
		IndiagramInfo GetOrCreateIndiagramInfo(long userId, long indiagramId, long version);
		void SetIndiagramImage(IndiagramInfo indiagramInfo, string filename, byte[] fileContent);
		void SetIndiagramSound(IndiagramInfo indiagramInfo, string filename, byte[] fileContent);
		Indiagram CreateIndiagram(long userId, long deviceId, IndiagramRequest indiagram);

		// collection versions
		bool HasIndiagramVersion(long userId, long version);
		Version CreateVersion(long userId);
		List<Version> GetVersions(long userId);
		List<Version> GetVersions(long userId, long startVersion);
	}
}
