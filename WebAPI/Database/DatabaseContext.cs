using System.Data.Entity;
using WebAPI.Models;

namespace WebAPI.Database
{
	public class DatabaseContext : DbContext
	{
		public DbSet<Indiagram> Indigrams { get; set; }

		public DbSet<User> Users { get; set; }
	}
}
