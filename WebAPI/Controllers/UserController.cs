using System.Web.Http;
using WebAPI.Database;

namespace WebAPI.Controllers
{
	[RoutePrefix("api/v1/users")]
	public class UserController : ApiController
	{
		[Route("login")]
		[HttpPost]
		public void Login([FromBody]string login, [FromBody]string password)
		{
			
		}

		[Route("register")]
		[HttpPost]
		public void Register([FromBody]string login, [FromBody]string password)
		{
			using (IDatabaseService database = new DatabaseService())
			{
				if (database.UserExists(login))
				{
					// return json error
				}
				else
				{
					database.RegisterUser(login, password);
					// return json success
				}
			}
		}
	}
}