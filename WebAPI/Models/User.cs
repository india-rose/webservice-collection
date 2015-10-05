using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
	public class User
	{
		[Key]
		public long Id { get; set; }

		[Index]
		public string Login { get; set; }

		public string Password { get; set; }
	}
}
