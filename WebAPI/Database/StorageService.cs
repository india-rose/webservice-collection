using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace WebAPI.Database
{
	public class StorageService : IStorageService
	{
		private const string IMAGES_CONTAINER = "images";
		private const string SOUNDS_CONTAINER = "sounds";

		private readonly CloudBlobClient _blobClient;

		public StorageService()
		{
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

			_blobClient = storageAccount.CreateCloudBlobClient();
		}

		public string UploadImage(string filename, byte[] fileBuffer)
		{
			CloudBlobContainer imageContainer = _blobClient.GetContainerReference(IMAGES_CONTAINER);
			
		}
	}
}
