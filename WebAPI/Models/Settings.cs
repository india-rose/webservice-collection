using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
	public class Settings
	{
		[Key]
		public long Id { get; set; }

		[Index]
		public long DeviceId { get; set; }

		public virtual Device Device { get; set; }

		public long VersionNumber { get; set; }

		public string SerializedSettings { get; set; }
	}
}
