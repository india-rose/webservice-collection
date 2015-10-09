namespace WebAPI.Common.Requests
{
	public class IndiagramRequest
	{
		public long Id { get; set; }

		public string Text { get; set; }

		public bool HasImage { get; set; }

		public string ImageHash { get; set; }

		public bool HasSound { get; set; }

		public string SoundHash { get; set; }

		public bool IsEnabled { get; set; }
	}
}
