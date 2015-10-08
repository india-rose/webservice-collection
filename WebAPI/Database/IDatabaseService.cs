using System;
using WebAPI.Models;

namespace WebAPI.Database
{
	public interface IDatabaseService : IDisposable
	{
		// user 
		bool UserLoginExists(string login);
		bool UserEmailExists(string email);
		User GetUserByLogin(string login);
		void RegisterUser(string login, string password);
		bool CheckAuthentification(string login, string password);
		
		// device
		void CreateDevice(User user, string name);
		bool HasDevice(User user, string name);
	}
}
