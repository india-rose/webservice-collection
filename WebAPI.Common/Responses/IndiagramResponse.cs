using System.Collections.Generic;

namespace WebAPI.Common.Responses
{
	public class IndiagramResponse
	{
		public long DatabaseId { get; set; }

		public string Text { get; set; }

		public bool HasImage { get; set; }

		public bool HasSound { get; set; }

		public string ImageHash { get; set; }

		public string SoundHash { get; set; }

		public string ImageFile { get; set; }

		public string SoundFile { get; set; }

		public int Position { get; set; }

		public bool IsEnabled { get; set; }

		public bool IsCategory { get; set; }

		public List<IndiagramResponse> Children { get; set; }

		public IndiagramResponse()
		{
			Children = new List<IndiagramResponse>();
		}
	}
}
