namespace WebAPI.Common.Requests
{
	public class UserRegisterRequest
	{
		public string Login { get; set; }

		public string Email { get; set; }

		public string Password { get; set; }
	}
}
