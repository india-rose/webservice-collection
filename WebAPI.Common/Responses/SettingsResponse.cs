namespace WebAPI.Common.Responses
{
	public class SettingsResponse
	{
		public string Settings { get; set; }

		public long Version { get; set; }

		public SettingsResponse()
		{
			
		}

		public SettingsResponse(string settings, long version)
		{
			Settings = settings;
			Version = version;
		}
	}
}
