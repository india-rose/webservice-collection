namespace WebAPI.Database
{
	public interface IStorageService
	{
		string UploadImage(string filename, byte[] fileBuffer);
	}
}
