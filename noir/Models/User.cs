using System.Collections.Generic;

namespace noir.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public decimal Balance { get; set; }
		public bool HasPlus { get; set; }
		public string Role { get; set; } = "user";

		public ICollection<Listing> Listings { get; set; } = new List<Listing>();
		public ICollection<Bid> Bids { get; set; } = new List<Bid>();
		public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
		public ICollection<Review> Reviews { get; set; } = new List<Review>();
		public ICollection<SavedCard> SavedCards { get; set; } = new List<SavedCard>();
		public Subscription? Subscription { get; set; }
	}
}
