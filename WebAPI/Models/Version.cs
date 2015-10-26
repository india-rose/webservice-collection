using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
	public class Version
	{
		[Key]
		public long Id { get; set; }

		[Index]
		public long UserId { get; set; }

		public bool IsOpen { get; set; }

		public virtual User User { get; set; }

		public long Number { get; set; }

		public DateTime Date { get; set; }
	}
}
