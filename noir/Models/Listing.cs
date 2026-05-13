using System;
using System.Collections.Generic;

namespace noir.Models
{
	public class Listing
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public bool IsAuction { get; set; }
		public bool IsRemoved { get; set; }
		public string Status { get; set; } = "active";
		public string Tags { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public string PhotoUrl { get; set; } = string.Empty;
		public string ArchiveRef { get; set; } = "";
		public int SellerId { get; set; }
		public User Seller { get; set; } = null!;

		public AuctionLot? AuctionLot { get; set; }
		public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
		public ICollection<Review> Reviews { get; set; } = new List<Review>();
	}
}