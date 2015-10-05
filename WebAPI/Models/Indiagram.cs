using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
	public class Indiagram
	{
		[Key]
		public long Id { get; set; }

		[Index]
		public long UserId { get; set; }
	}

	public class IndiagramInfo
	{
		[Key]
		public long Id { get; set; }

		[Index]
		public long IndiagramId { get; set; }

		public long Version { get; set; }

		public int ParentId { get; set; }

		public string Text { get; set; }

		public string SoundPath { get; set; }

		public string ImagePath { get; set; }

		public bool IsCategory { get; set; }
	}
}
