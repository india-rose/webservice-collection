using System.Collections.Generic;

namespace WebAPI.Common.Responses
{
	public class IndiagramResponse
	{
		public long DatabaseId { get; set; }

		public string Text { get; set; }

		public string ImagePath { get; set; }

		public string SoundPath { get; set; }

		public bool IsEnabled { get; set; }

		public bool IsCategory { get; set; }

		public List<IndiagramResponse> Children { get; set; }

		public IndiagramResponse()
		{
			Children = new List<IndiagramResponse>();
		}
	}
}
