using System.Data.Entity;
using WebAPI.Models;

namespace WebAPI.Database
{
	public class DatabaseContext : DbContext
	{
		public DbSet<Indiagram> Indiagrams { get; set; }

		public DbSet<Version> Versions { get; set; }

		public DbSet<User> Users { get; set; }

		public DbSet<Settings> Settings { get; set; }

		public DbSet<Device> Devices { get; set; }
	}
}
