﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebAPI.Database;

namespace WebAPI.Models
{
	public class Indiagram
	{
		[Key]
		public long Id { get; set; }

		[Index]
		public long UserId { get; set; }

		public virtual User User { get; set; }

		[ForeignKey("LastIndiagramInfo")]
		public long LastIndiagramInfoId { get; set; }

		public virtual IndiagramInfo LastIndiagramInfo { get; set; } 

		public virtual List<IndiagramInfo> Infos { get; set; }
	}

	public class IndiagramInfo
	{
		[Key]
		public long Id { get; set; }

		[Index]
		public long IndiagramId { get; set; }

		public virtual List<IndiagramState> States { get; set; } 

		public virtual Indiagram Indiagram { get; set; }

		public long Version { get; set; }

		public long ParentId { get; set; }

		public int Position { get; set; }

		public string Text { get; set; }

		public string SoundPath { get; set; }

		public string ImagePath { get; set; }

		public string SoundHash { get; set; }

		public string ImageHash { get; set; }

		public bool IsCategory { get; set; }
	}

	public class IndiagramState
	{
		[Key]
		public long Id { get; set; }

		[Index]
		public long IndiagramInfoId { get; set; }

		public virtual IndiagramInfo IndiagramInfo { get; set; }

		[Index]
		public long DeviceId { get; set; }

		public virtual Device Device { get; set; }

		public bool IsEnabled { get; set; }
	}
}
