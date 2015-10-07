using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Database;
using WebAPI.Extensions;

namespace WebAPI.Controllers
{
	[RoutePrefix("api/v1/users")]
	public class UserController : ApiController
	{
		private const int ERROR_CODE_LOGIN_EXISTS = 100;
		private const int ERROR_CODE_EMAIL_EXISTS = 101;

		private const string ERROR_TEXT_LOGIN_EXISTS = "Login already exists";
		private const string ERROR_TEXT_EMAIL_EXISTS = "Email already registered";

		[Route("login")]
		[HttpPost]
		public void Login([FromBody]string login, [FromBody]string password)
		{
			
		}

		[Route("register")]
		[HttpPost]
		public HttpResponseMessage Register([FromBody]string login, [FromBody]string email, [FromBody]string password)
		{
			if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing fields in request");
			}

			using (IDatabaseService database = new DatabaseService())
			{
				if (database.UserLoginExists(login))
				{
					return Request.CreateCustomError(ERROR_CODE_LOGIN_EXISTS, ERROR_TEXT_LOGIN_EXISTS);
				}
				if (database.UserEmailExists(email))
				{
					return Request.CreateCustomError(ERROR_CODE_EMAIL_EXISTS, ERROR_TEXT_EMAIL_EXISTS);
				}
				
				database.RegisterUser(login, password);
				return Request.CreateEmptyGoodReponse();
			}
		}
	}
}