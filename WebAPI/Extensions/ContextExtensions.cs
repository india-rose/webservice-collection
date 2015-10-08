using System.Web.Http.Controllers;
using WebAPI.Filters;
using WebAPI.Models;

namespace WebAPI.Extensions
{
	public static class ContextExtensions
	{
		public static User GetAuthenticatedUser(this HttpRequestContext context)
		{
			AuthentificationPrincipal profile = context.Principal as AuthentificationPrincipal;
			if (profile == null)
			{
				return null;
			}
			return profile.User;
		}
	}
}
