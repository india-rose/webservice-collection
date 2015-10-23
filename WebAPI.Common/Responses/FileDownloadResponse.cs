namespace WebAPI.Common.Responses
{
	public class FileDownloadResponse
	{
		public string FileName { get; set; }

		public byte[] Content { get; set; }
	}
}
