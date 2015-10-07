using System;
using WebAPI.Models;

namespace WebAPI.Database
{
	public interface IDatabaseService : IDisposable
	{
		bool UserLoginExists(string login);
		bool UserEmailExists(string email);
		User GetUserByLogin(string login);
		void RegisterUser(string login, string password);
	}
}
