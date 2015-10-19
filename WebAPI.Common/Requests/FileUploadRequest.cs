namespace WebAPI.Common.Requests
{
	public class FileUploadRequest
	{
		public string Filename { get; set; }

		public byte[] Content { get; set; }
	}
}
