using System;

namespace WebAPI.Common.Responses
{
	public class VersionResponse
	{
		public long Version { get; set; }

		public DateTime Date { get; set; }

		public VersionResponse()
		{
			
		}

		public VersionResponse(long version, DateTime date)
		{
			Version = version;
			Date = date;
		}
	}
}
