using System;

namespace WebAPI.Common.Responses
{
	public class SettingsResponse
	{
		public string Settings { get; set; }

		public DateTime Date { get; set; }

		public long Version { get; set; }

		public SettingsResponse()
		{
			
		}

		public SettingsResponse(string settings, long version, DateTime date)
		{
			Settings = settings;
			Version = version;
			Date = date;
		}
	}
}
