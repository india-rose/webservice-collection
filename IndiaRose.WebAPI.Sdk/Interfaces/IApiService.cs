using System.Threading.Tasks;
using IndiaRose.WebAPI.Sdk.Results;

namespace IndiaRose.WebAPI.Sdk.Interfaces
{
	public interface IApiService
	{
		/// <summary>
		/// Register a user on the api.
		/// Result code : 
		///		0 => everything went fine
		///		1 => server error
		///		2 => fields not correct 
		///		100 => login already used 
		///		101 => email already used
		/// </summary>
		/// <param name="login">user login</param>
		/// <param name="email">user email</param>
		/// <param name="password">user password (sha 256)</param>
		/// <returns>status code</returns>
		Task<UserStatusCode> RegisterUserAsync(string login, string email, string password);

		/// <summary>
		/// Check if user login information are corrects
		/// Result code : 
		///		0 => authentification ok
		///		1 => server error
		///		2 => fields incorrect
		///		2 => login/password incorrect 
		/// </summary>
		/// <param name="login">user login</param>
		/// <param name="password">user password (sha 256)</param>
		/// <returns>status code</returns>
		Task<UserStatusCode> LoginUserAsync(string login, string password);
	}
}
