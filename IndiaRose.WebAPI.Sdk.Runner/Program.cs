﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using IndiaRose.WebAPI.Sdk.Interfaces;
using IndiaRose.WebAPI.Sdk.Models;
using IndiaRose.WebAPI.Sdk.Results;
using IndiaRose.WebAPI.Sdk.Services;
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

			string passwd = ComputePasswordHash("rose");
			UserStatusCode result = await api.RegisterUserAsync("india", "india@gmail.com", passwd);

			Console.WriteLine("Result Code {0}", result);
			
			result = await api.LoginUserAsync("india", passwd);

			Console.WriteLine("Result Code {0}", result);

			UserInfo badUserInfo = new UserInfo {Login = "test", Password = "test"};
			UserInfo userInfo = new UserInfo{Login = "india", Password = passwd};
			DeviceStatusCode dResult = await api.CreateDeviceAsync(badUserInfo, "test123");
			Console.WriteLine("Create bad device : {0}", dResult);
			dResult = await api.CreateDeviceAsync(userInfo, "test123");
			Console.WriteLine("Create good device : {0}", dResult);

			List<DeviceResponse> devices = new List<DeviceResponse>();
			dResult = await api.ListDevicesAsync(userInfo, devices);
			Console.WriteLine("Display devices : {0}", dResult);
			devices.ForEach(x => Console.WriteLine("\t{0}", x.Name));

			dResult = await api.RenameDeviceAsync(userInfo, "test123", "test456");
			Console.WriteLine("Rename device : {0}", dResult);

			dResult = await api.ListDevicesAsync(userInfo, devices);
			Console.WriteLine("Display devices : {0}", dResult);
			devices.ForEach(x => Console.WriteLine("\t{0}", x.Name));



			//Console.WriteLine("Got result {0}", result);
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
	}
}
