using System.Security.Principal;

namespace WebAPI.Filters
{
	public class AuthentificationIdentity : GenericIdentity
	{
		public string Password { get; set; }

		public AuthentificationIdentity(string login, string password) : base(login)
		{
			Password = password;
		}
	}
}