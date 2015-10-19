using WebAPI.Models;

namespace WebAPI.Database
{
	public interface IStorageService
	{
		bool UploadImage(IndiagramInfo indiagram, byte[] fileBuffer);

		bool UploadSound(IndiagramInfo indiagram, byte[] fileBuffer);

		byte[] DownloadImage(long indiagramId, long version);

		byte[] DownloadSound(long indiagramId, long version);
	}
}
