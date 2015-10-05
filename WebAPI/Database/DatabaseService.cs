using System;
using System.Linq;
using WebAPI.Models;

namespace WebAPI.Database
{
	public class DatabaseService : IDatabaseService
	{
		private readonly DatabaseContext _context;

		public DatabaseService()
		{
			_context = new DatabaseContext();
		}
		
		public void Dispose()
		{
			if (_context != null)
			{
				_context.Dispose();
			}
		}

		public bool UserExists(string login)
		{
			return _context.Users.Any(x => x.Login == login);
		}

		public User GetUserByLogin(string login)
		{
			return _context.Users.FirstOrDefault(x => x.Login == login);
		}

		public void RegisterUser(string login, string password)
		{
			_context.Users.Add(new User
			{
				Login = login,
				Password = password
			});
			_context.SaveChanges();
		}
	}
}
