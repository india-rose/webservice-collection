using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Common.Requests;
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
		public HttpResponseMessage Login([FromBody] UserLoginRequest userInfo)
		{
			if (string.IsNullOrWhiteSpace(userInfo.Login) || string.IsNullOrWhiteSpace(userInfo.Password))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				if (!database.CheckAuthentification(userInfo.Login, userInfo.Password))
				{
					return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid credentials");
				}
				
				return Request.CreateEmptyGoodReponse();
			}
		}

		[Route("register")]
		[HttpPost]
		public HttpResponseMessage Register([FromBody] UserRegisterRequest userInfo)
		{
			if (string.IsNullOrWhiteSpace(userInfo.Login) || string.IsNullOrWhiteSpace(userInfo.Email) || string.IsNullOrWhiteSpace(userInfo.Password))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				if (database.UserLoginExists(userInfo.Login))
				{
					return Request.CreateCustomError(ERROR_CODE_LOGIN_EXISTS, ERROR_TEXT_LOGIN_EXISTS);
				}
				if (database.UserEmailExists(userInfo.Email))
				{
					return Request.CreateCustomError(ERROR_CODE_EMAIL_EXISTS, ERROR_TEXT_EMAIL_EXISTS);
				}
				
				database.RegisterUser(userInfo.Login, userInfo.Email, userInfo.Password);
				return Request.CreateEmptyGoodReponse();
			}
		}
	}
}