using System;
using System.Collections.Generic;

namespace noir.Models
{
	public class AuctionLot
	{
		public int Id { get; set; }
		public decimal StartPrice { get; set; }
		public decimal CurrentPrice { get; set; }
		public decimal MinBidStep { get; set; }  // ← ДОБАВЛЕНО
		public DateTime EndDate { get; set; }
		public bool IsEnded { get; set; }
		public int ListingId { get; set; }
		public Listing Listing { get; set; } = null!;

		public ICollection<Bid> Bids { get; set; } = new List<Bid>();
	}
}