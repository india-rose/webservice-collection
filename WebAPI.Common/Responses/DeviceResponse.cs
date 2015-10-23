namespace WebAPI.Common.Responses
{
	public class DeviceResponse
	{
		public string Name { get; set; }

		public DeviceResponse()
		{
			
		}

		public DeviceResponse(string name)
		{
			Name = name;
		}
	}
}
