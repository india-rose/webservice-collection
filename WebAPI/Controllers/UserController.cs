using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using WebAPI.Common.Requests;
using WebAPI.Common.Responses;
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

		/// <summary>
		/// Try to login a user with login and password.
		/// </summary>
		/// <param name="userInfo">User info, the password must be sha 256 hashed.</param>
		/// <returns></returns>
		[Route("login")]
		[HttpPost]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Invalid data in the request", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.Unauthorized, "Invalid login/password couple", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.OK, "Login success", typeof(RequestResult))]
		public HttpResponseMessage Login([FromBody] UserLoginRequest userInfo)
		{
			if (string.IsNullOrWhiteSpace(userInfo?.Login) || string.IsNullOrWhiteSpace(userInfo.Password))
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

		/// <summary>
		/// Method to create a new user.
		/// </summary>
		/// <remarks>
		/// Could receive error Code 100 if login already exists.
		/// Error Code 101 email already exists.
		/// </remarks>
		/// <param name="userInfo">User info.</param>
		/// <returns></returns>
		[Route("register")]
		[HttpPost]
		[SwaggerResponse(HttpStatusCode.BadRequest, "Invalid data in the request", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.Conflict, "Login or email already exists, check errorCode for more informations.", typeof(RequestResult))]
		[SwaggerResponse(HttpStatusCode.OK, "Register success", typeof(RequestResult))]
		public HttpResponseMessage Register([FromBody] UserRegisterRequest userInfo)
		{
			if (string.IsNullOrWhiteSpace(userInfo?.Login) || string.IsNullOrWhiteSpace(userInfo.Email) || string.IsNullOrWhiteSpace(userInfo.Password))
			{
				return Request.CreateBadRequestResponse();
			}

			using (IDatabaseService database = new DatabaseService())
			{
				if (database.UserLoginExists(userInfo.Login))
				{
					return Request.CreateCustomError(HttpStatusCode.Conflict, ERROR_CODE_LOGIN_EXISTS, ERROR_TEXT_LOGIN_EXISTS);
				}
				if (database.UserEmailExists(userInfo.Email))
				{
					return Request.CreateCustomError(HttpStatusCode.Conflict, ERROR_CODE_EMAIL_EXISTS, ERROR_TEXT_EMAIL_EXISTS);
				}
				
				database.RegisterUser(userInfo.Login, userInfo.Email, HashPassword(userInfo.Password));
				return Request.CreateEmptyGoodReponse();
			}
		}

		private string HashPassword(string passwd)
		{
			using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
			{
				byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwd));
				string hex = BitConverter.ToString(hash);
				return hex.Replace("-", "").ToUpperInvariant();
			}
		}
	}
}