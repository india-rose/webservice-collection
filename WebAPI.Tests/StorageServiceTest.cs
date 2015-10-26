using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAPI.Database;
using WebAPI.Models;

namespace WebAPI.Tests
{
	[TestClass]
	public class StorageServiceTest
	{
		private readonly byte[] _buffer = {42, 51, 73, 0x2a};
		private readonly byte[] _emptyBuffer = {};
		private readonly byte[] _nullBuffer = null;

		private readonly IndiagramInfo _info1 = new IndiagramInfo
		{
			Id = 1, Version = 1
		};
		private readonly IndiagramInfo _info2 = new IndiagramInfo
		{
			Id = 2,
			Version = 1
		};

		[TestMethod]
		public void TestUploadSound()
		{
			Initializer.Reset();
			StorageService storage = new StorageService();

			Assert.IsTrue(storage.UploadSound(_info1, _buffer));
			Assert.IsTrue(storage.UploadSound(_info2, _buffer));

			Assert.IsFalse(storage.UploadSound(_info1, _buffer));
			Assert.IsFalse(storage.UploadSound(_info1, _emptyBuffer));
		}

		[TestMethod]
		public void TestUploadSound_Empty()
		{
			Initializer.Reset();
			StorageService storage = new StorageService();

			Assert.IsTrue(storage.UploadSound(_info1, _emptyBuffer));
			Assert.IsTrue(storage.UploadSound(_info2, _emptyBuffer));

			Assert.IsFalse(storage.UploadSound(_info1, _buffer));
			Assert.IsFalse(storage.UploadSound(_info1, _emptyBuffer));
		}

		[TestMethod]
		public void TestUploadSound_Null()
		{
			Initializer.Reset();
			StorageService storage = new StorageService();

			Assert.IsFalse(storage.UploadSound(_info1, _nullBuffer));
			Assert.IsFalse(storage.UploadSound(_info2, _nullBuffer));

			Assert.IsTrue(storage.UploadSound(_info1, _emptyBuffer));
			Assert.IsFalse(storage.UploadSound(_info1, _nullBuffer));
		}

		[TestMethod]
		public void TestUploadImage()
		{
			
		}

		[TestMethod]
		public void TestUploadImage_Empty()
		{
			
		}

		[TestMethod]
		public void TestUploadImage_Null()
		{

		}
	}
}
