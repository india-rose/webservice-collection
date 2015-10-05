using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
	public class Device
	{
		[Key]
		public long Id { get; set; }

		[Index]
		public long UserId { get; set; }

		public string DeviceName { get; set; }
	}
}
