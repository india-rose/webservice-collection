using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
	public class User
	{
		[Key]
		public long Id { get; set; }

		public string Login { get; set; }

		public string Email { get; set; }

		public string Password { get; set; }
	}
}
