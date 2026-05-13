using System;

namespace noir.Models
{
	public class UserSave
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public User User { get; set; } = null!;
		public int ListingId { get; set; }
		public Listing Listing { get; set; } = null!;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}