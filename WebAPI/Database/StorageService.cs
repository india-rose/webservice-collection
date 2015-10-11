using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using WebAPI.Models;

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

		private bool UploadFile(IndiagramInfo indiagram, byte[] fileBuffer, string containerName)
		{
			try
			{
				CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
				container.CreateIfNotExists(BlobContainerPublicAccessType.Container);

				string filename = string.Format("{0}_{1}", indiagram.IndiagramId, indiagram.Version);

				if (container.ListBlobs(filename).Any())
				{
					return false;
				}
				CloudBlockBlob blob = container.GetBlockBlobReference(filename);
				blob.UploadFromByteArray(fileBuffer, 0, fileBuffer.Length);
			}
			catch (Exception e)
			{
				Trace.TraceError("Exception while uploading file to container {0} : {1}\n{2}", containerName, e.Message, e.StackTrace);
				return false;
			}
			return true;
		}

		public bool UploadSound(IndiagramInfo indiagram, byte[] fileBuffer)
		{
			return UploadFile(indiagram, fileBuffer, SOUNDS_CONTAINER);
		}

		public bool UploadImage(IndiagramInfo indiagram, byte[] fileBuffer)
		{
			return UploadFile(indiagram, fileBuffer, IMAGES_CONTAINER);
		}
	}
}
