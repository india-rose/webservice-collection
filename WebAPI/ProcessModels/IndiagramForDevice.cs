namespace WebAPI.ProcessModels
{
	public class IndiagramForDevice
	{
		public long Id { get; set; }

		public long Version { get; set; }

		public long ParentId { get; set; }

		public string Text { get; set; }

		public string SoundHash { get; set; }

		public string ImageHash { get; set; }

		public int Position { get; set; }

		public bool IsCategory { get; set; }

		public bool IsEnabled { get; set; }
	}
}
