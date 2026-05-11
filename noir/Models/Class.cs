using System;
using System.Collections.Generic;

namespace noir.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Username { get; set; } = "";
		public string Email { get; set; } = "";
		public string PasswordHash { get; set; } = "";
		public string Role { get; set; } = "user";
		public bool HasPlus { get; set; } = false;
		public decimal Balance { get; set; } = 0;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public Subscription? Subscription { get; set; }
		public ICollection<Listing> Listings { get; set; } = new List<Listing>();
		public ICollection<SavedCard> SavedCards { get; set; } = new List<SavedCard>();
		public ICollection<Bid> Bids { get; set; } = new List<Bid>();
		public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
		public ICollection<Review> Reviews { get; set; } = new List<Review>();
	}

	public class Listing
	{
		public int Id { get; set; }
		public string Title { get; set; } = "";
		public string Description { get; set; } = "";
		public decimal Price { get; set; }
		public string Type { get; set; } = "sale";
		public string Status { get; set; } = "active";
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public int SellerId { get; set; }
		public User Seller { get; set; } = null!;

		public AuctionLot? AuctionLot { get; set; }
		public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
		public ICollection<Review> Reviews { get; set; } = new List<Review>();
	}

	public class AuctionLot
	{
		public int Id { get; set; }
		public decimal StartPrice { get; set; }
		public decimal CurrentPrice { get; set; }
		public DateTime EndTime { get; set; }
		public bool IsEnded { get; set; } = false;

		public int ListingId { get; set; }
		public Listing Listing { get; set; } = null!;

		public ICollection<Bid> Bids { get; set; } = new List<Bid>();
	}

	public class Bid
	{
		public int Id { get; set; }
		public decimal Amount { get; set; }
		public DateTime PlacedAt { get; set; } = DateTime.UtcNow;

		public int UserId { get; set; }
		public User User { get; set; } = null!;

		public int LotId { get; set; }
		public AuctionLot Lot { get; set; } = null!;
	}

	public class Purchase
	{
		public int Id { get; set; }
		public decimal Amount { get; set; }
		public DateTime PaidAt { get; set; } = DateTime.UtcNow;

		public int BuyerId { get; set; }
		public User Buyer { get; set; } = null!;

		public int ListingId { get; set; }
		public Listing Listing { get; set; } = null!;
	}

	public class Subscription
	{
		public int Id { get; set; }
		public string Plan { get; set; } = "plus";
		public DateTime ActiveSince { get; set; } = DateTime.UtcNow;
		public DateTime ExpiresAt { get; set; }
		public decimal Price { get; set; } = 10.99m;

		public int UserId { get; set; }
		public User User { get; set; } = null!;
	}

	public class SavedCard
	{
		public int Id { get; set; }
		public string Network { get; set; } = "";
		public string Last4 { get; set; } = "";
		public string Expiry { get; set; } = "";

		public int UserId { get; set; }
		public User User { get; set; } = null!;
	}

	public class Review
	{
		public int Id { get; set; }
		public string Body { get; set; } = "";
		public int Stars { get; set; } = 5;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public int ReviewerId { get; set; }
		public User Reviewer { get; set; } = null!;

		public int ListingId { get; set; }
		public Listing Listing { get; set; } = null!;
	}
}