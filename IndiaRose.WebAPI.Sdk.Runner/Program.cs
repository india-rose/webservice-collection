﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using IndiaRose.WebAPI.Sdk.Interfaces;
using IndiaRose.WebAPI.Sdk.Models;
using IndiaRose.WebAPI.Sdk.Results;
using IndiaRose.WebAPI.Sdk.Services;
using Newtonsoft.Json;
using WebAPI.Common.Requests;
using WebAPI.Common.Responses;

namespace IndiaRose.WebAPI.Sdk.Runner
{
	class Program
	{
		//private const string HOST = "http://indiarose.azurewebsites.net";
		private const string HOST = "http://localhost:59911";

		static void Main(string[] args)
		{
			Run();

			//Console.ReadKey();
			Console.ReadKey();
		}

		static async void Run()
		{
			IApiService api = new ApiService(new RequestService(), new ApiLogger(), HOST);

			Console.WriteLine("Running...");

			string passwd = ComputePasswordHash("rose");
			UserStatusCode result;
			/*
			result = await api.RegisterUserAsync("india", "india@gmail.com", "rose");

			Console.WriteLine("Result Code {0}", result);
			*/
			result = await api.LoginUserAsync("india", passwd);

			Console.WriteLine("Login Result Code {0}", result);

			UserInfo badUserInfo = new UserInfo {Login = "test", Password = "test"};
			UserInfo userInfo = new UserInfo{Login = "india", Password = passwd};
			DeviceStatusCode dResult = await api.CreateDeviceAsync(badUserInfo, "test123");
			Console.WriteLine("Create bad device : {0}", dResult);
			dResult = await api.CreateDeviceAsync(userInfo, "azerty");
			Console.WriteLine("Create good device : {0}", dResult);

			ApiResult<DeviceStatusCode, List<DeviceResponse>> deviceListResult = await api.ListDevicesAsync(userInfo);
			Console.WriteLine("Display devices : {0}", deviceListResult.Status);
			deviceListResult.Content.ForEach(x => Console.WriteLine("\t{0}", x.Name));

			dResult = await api.RenameDeviceAsync(userInfo, "azerty", "uiopml");
			Console.WriteLine("Rename device : {0}", dResult);

			deviceListResult = await api.ListDevicesAsync(userInfo);
			Console.WriteLine("Display devices : {0}", deviceListResult.Status);
			deviceListResult.Content.ForEach(x => Console.WriteLine("\t{0}", x.Name));

			DeviceInfo device = new DeviceInfo
			{
				Name = "test123"
			};
			var sResult = await api.GetLastSettingsAsync(userInfo, device);
			Console.WriteLine("Get Settings : {0}", sResult.Status);
			if (sResult.Content != null)
			{
				Console.WriteLine("\tDate : {0}", sResult.Content.Date);
				Console.WriteLine("\tVersion : {0}", sResult.Content.Version);
				Console.WriteLine("\tData : {0}", sResult.Content.Settings);
			}

			var updateResult = await api.UpdateSettingsAsync(userInfo, device, JsonConvert.SerializeObject(new SettingsModel()));
			Console.WriteLine("update settings : {0}", updateResult);

			sResult = await api.GetLastSettingsAsync(userInfo, device);
			Console.WriteLine("Get Settings : {0}", sResult.Status);
			if (sResult.Content != null)
			{
				Console.WriteLine("\tDate : {0}", sResult.Content.Date);
				Console.WriteLine("\tVersion : {0}", sResult.Content.Version);
				Console.WriteLine("\tData : {0}", sResult.Content.Settings);
			}

			var slResult = await api.GetSettingsListAsync(userInfo, device);
			Console.WriteLine("Get list of settings => {0}", slResult.Status);
			if (slResult.Content != null)
			{
				foreach (var settings in slResult.Content)
				{
					Console.WriteLine("\tDate : {0}", settings.Date);
					Console.WriteLine("\tVersion : {0}", settings.Version);
					Console.WriteLine("\tData : {0}", settings.Settings);
				}
				Console.WriteLine();
			}
			Console.WriteLine();


			sResult = await api.GetVersionSettingsAsync(userInfo, device, 2);
			Console.WriteLine("Get Settings for version 2: {0}", sResult.Status);
			if (sResult.Content != null)
			{
				Console.WriteLine("\tDate : {0}", sResult.Content.Date);
				Console.WriteLine("\tVersion : {0}", sResult.Content.Version);
				Console.WriteLine("\tData : {0}", sResult.Content.Settings);
			}
			
			//Console.WriteLine("Got result {0}", result);

			var vResult = await api.GetVersions(userInfo, device);
			Console.WriteLine("GetVersions : {0}", vResult.Status);
			if (vResult.Content != null)
			{
				foreach (var v in vResult.Content)
				{
					Console.WriteLine("\tVersion : {0}\n\tDate : {1}", v.Version, v.Date);
				}
			}

			vResult = await api.GetVersions(userInfo, device, 2);
			Console.WriteLine("GetVersionsFrom(2) : {0}", vResult.Status);
			if (vResult.Content != null)
			{
				foreach (var v in vResult.Content)
				{
					Console.WriteLine("\tVersion : {0}\n\tDate : {1}", v.Version, v.Date);
				}
			}

			var vCreateResult = await api.CreateVersion(userInfo, device);
			Console.WriteLine("CreateVersion : {0}", vCreateResult.Status);
			if (vCreateResult.Content != null)
			{
				var v = vCreateResult.Content;
				Console.WriteLine("\tVersion : {0}\n\tDate : {1}", v.Version, v.Date);
			}
			else
			{
				return;
			}

			var iCreateResult = await api.UpdateIndiagram(userInfo, device, new IndiagramRequest
			{
				Id = -1,
				IsCategory = false,
				IsEnabled = false,
				ParentId = -1,
				Position = 1,
				Text = "Test image",
				Version = vCreateResult.Content.Version
			});
			Console.WriteLine("Create indiagram : {0}", iCreateResult.Status);
			if (iCreateResult.Content != null)
			{
				IndiagramResponse i = iCreateResult.Content;
				Console.WriteLine("\tDatabaseId : {0}", i.DatabaseId);
				Console.WriteLine("\tIsCategory : {0}", i.IsCategory);
				Console.WriteLine("\tIsEnabled : {0}", i.IsEnabled);
				Console.WriteLine("\tText : {0}", i.Text);
				Console.WriteLine("\tPosition : {0}", i.Position);
				Console.WriteLine("\tHasImage : {0}", i.HasImage);
				Console.WriteLine("\tHasSound : {0}", i.HasSound);
				Console.WriteLine("\tImageHash : {0}", i.ImageHash);
				Console.WriteLine("\tSoundHash : {0}", i.SoundHash);
				Console.WriteLine("\tImageFile : {0}", i.ImageFile);
				Console.WriteLine("\tSoundFile : {0}", i.SoundFile);
			}
			else
			{
				return;
			}

			var uploadResult = await api.UploadImage(userInfo, device, iCreateResult.Content.DatabaseId, vCreateResult.Content.Version, "robot.png", ReadImage());
			Console.WriteLine("Upload image : {0}", uploadResult);

			iCreateResult = await api.GetIndiagram(userInfo, device, iCreateResult.Content.DatabaseId);
			Console.WriteLine("Get indiagram : {0}", iCreateResult.Status);
			if (iCreateResult.Content != null)
			{
				IndiagramResponse i = iCreateResult.Content;
				Console.WriteLine("\tDatabaseId : {0}", i.DatabaseId);
				Console.WriteLine("\tIsCategory : {0}", i.IsCategory);
				Console.WriteLine("\tIsEnabled : {0}", i.IsEnabled);
				Console.WriteLine("\tText : {0}", i.Text);
				Console.WriteLine("\tPosition : {0}", i.Position);
				Console.WriteLine("\tHasImage : {0}", i.HasImage);
				Console.WriteLine("\tHasSound : {0}", i.HasSound);
				Console.WriteLine("\tImageHash : {0}", i.ImageHash);
				Console.WriteLine("\tSoundHash : {0}", i.SoundHash);
				Console.WriteLine("\tImageFile : {0}", i.ImageFile);
				Console.WriteLine("\tSoundFile : {0}", i.SoundFile);
			}
			else
			{
				return;
			}

			var downloadResult = await api.GetImage(userInfo, device, iCreateResult.Content.DatabaseId, vCreateResult.Content.Version);
			Console.WriteLine("Download image : {0}", downloadResult.Status);
			if (downloadResult.Content != null)
			{
				File.WriteAllBytes(string.Format("output-{0}", downloadResult.Content.FileName), downloadResult.Content.Content);
				Console.WriteLine("Image output write to output-{0}", downloadResult.Content.FileName);
			}
		}

		private static string ComputePasswordHash(string input)
		{
			using (SHA256Managed sha = new SHA256Managed())
			{
				byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
				string hex = BitConverter.ToString(hash);
				return hex.Replace("-", "").ToUpperInvariant();
			}
		}

		private static byte[] ReadImage()
		{
			return File.ReadAllBytes("robot_image.png");
		}
	}
}
