using System;

namespace noir.Models
{
	public class Purchase
	{
		public int Id { get; set; }
		public decimal Amount { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public int BuyerId { get; set; }
		public User Buyer { get; set; } = null!;
		public int ListingId { get; set; }
		public Listing Listing { get; set; } = null!;
	}
}