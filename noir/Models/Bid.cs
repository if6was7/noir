using System;

namespace noir.Models
{
	public class Bid
	{
		public int Id { get; set; }
		public decimal Amount { get; set; }
		public DateTime CreatedAt { get; set; }
		public int LotId { get; set; }
		public AuctionLot Lot { get; set; } = null!;
		public int UserId { get; set; }
		public User User { get; set; } = null!;
	}
}
