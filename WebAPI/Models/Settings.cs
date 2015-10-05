using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
	public class Settings
	{
		[Key]
		public int Id { get; set; }

		[Index]
		public int DeviceId { get; set; }

		public string SerializedSettings { get; set; }
	}
}
