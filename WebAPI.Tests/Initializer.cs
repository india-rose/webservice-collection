using System;
using System.Data.Entity;
using System.Diagnostics;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using WebAPI.Database;

namespace WebAPI.Tests
{
	public static class Initializer
	{
		public static void Reset()
		{
			IDatabaseInitializer<DatabaseContext> init = new DropCreateDatabaseAlways<DatabaseContext>();
			System.Data.Entity.Database.SetInitializer(init);
			init.InitializeDatabase(new DatabaseContext());

			string connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
#if DEBUG
			Console.WriteLine("StorageConnectionString : {0}", connectionString);
			Trace.TraceInformation("StorageConnectionString : {0}", connectionString);
#endif
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
			CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

			foreach(string containerName in new []{"images", "sounds"})
			{
				CloudBlobContainer container = blobClient.GetContainerReference(containerName);
				if (container.Exists())
				{
					container.Delete();
				}
			}
		}
	}
}
