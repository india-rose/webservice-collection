namespace WebAPI.Common.Requests
{
	public class IndiagramRequest
	{
		public long Id { get; set; }

		public long Version { get; set; }

		public string Text { get; set; }

		public long ParentId { get; set; }

		public int Position { get; set; }

		public bool IsEnabled { get; set; }

		public bool IsCategory { get; set; }
	}
}
