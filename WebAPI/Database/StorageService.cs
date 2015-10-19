using System;
using System.Diagnostics;
using System.IO;
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

		private byte[] DownloadFile(long indiagramId, long version, string containerName)
		{
			try
			{
				CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
				if (!container.Exists())
				{
					return null;
				}

				string filename = string.Format("{0}_{1}", indiagramId, version);

				CloudBlockBlob blob = container.GetBlockBlobReference(filename);
				if (!blob.Exists())
				{
					return null;
				}
				MemoryStream outputStream = new MemoryStream();
				blob.DownloadToStream(outputStream);

				return outputStream.ToArray();
			}
			catch (Exception e)
			{
				Trace.TraceError("Exception while downloading file from container {0} : {1}\n{2}", containerName, e.Message, e.StackTrace);
			}
			return null;
		}

		public bool UploadSound(IndiagramInfo indiagram, byte[] fileBuffer)
		{
			return UploadFile(indiagram, fileBuffer, SOUNDS_CONTAINER);
		}

		public bool UploadImage(IndiagramInfo indiagram, byte[] fileBuffer)
		{
			return UploadFile(indiagram, fileBuffer, IMAGES_CONTAINER);
		}

		public byte[] DownloadImage(long indiagramId, long version)
		{
			return DownloadFile(indiagramId, version, IMAGES_CONTAINER);
		}

		public byte[] DownloadSound(long indiagramId, long version)
		{
			return DownloadFile(indiagramId, version, SOUNDS_CONTAINER);
		}
	}
}
