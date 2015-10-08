using System.Security.Principal;
using WebAPI.Models;

namespace WebAPI.Filters
{
	public class AuthentificationPrincipal : GenericPrincipal
	{
		public User User { get; set; }

		public Device Device { get; set; }

		public AuthentificationPrincipal(User user, params string[] roles) : base(new AuthentificationIdentity(user.Login, user.Password), roles ?? new string[]{})
		{
			User = user;
		}
	}
}
