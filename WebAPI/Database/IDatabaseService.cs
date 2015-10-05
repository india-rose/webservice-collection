using System;
using WebAPI.Models;

namespace WebAPI.Database
{
	public interface IDatabaseService : IDisposable
	{
		bool UserExists(string login);
		User GetUserByLogin(string login);
		void RegisterUser(string login, string password);
	}
}
