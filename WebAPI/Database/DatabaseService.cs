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

		#region users

		public bool UserLoginExists(string login)
		{
			return _context.Users.Any(x => x.Login == login);
		}

		public bool UserEmailExists(string email)
		{
			return _context.Users.Any(x => x.Email == email);
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
				Password = password.ToUpperInvariant()
			});
			_context.SaveChanges();
		}

		public bool CheckAuthentification(string login, string password)
		{
			User user = GetUserByLogin(login);

			if (user == null)
			{
				return false;
			}

			return user.Password.Equals(password.ToUpperInvariant());
		}

		#endregion

		#region devices

		public bool HasDevice(User user, string name)
		{
			return _context.Devices.Where(x => x.UserId == user.Id).FirstOrDefault(x => x.DeviceName == name) != null;
		}

		public void CreateDevice(User user, string name)
		{
			_context.Devices.Add(new Device
			{
				DeviceName = name,
				UserId = user.Id
			});
			_context.SaveChanges();
		}

		#endregion
	}
}
