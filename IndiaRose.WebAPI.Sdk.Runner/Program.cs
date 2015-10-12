using System;
using System.Security.Cryptography;
using System.Text;
using IndiaRose.WebAPI.Sdk.Interfaces;
using IndiaRose.WebAPI.Sdk.Results;
using IndiaRose.WebAPI.Sdk.Services;

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
