using System;

namespace noir.Models
{
	public class Subscription
	{
		public int Id { get; set; }
		public bool IsActive { get; set; }
		public DateTime ActiveSince { get; set; }
		public DateTime EndDate { get; set; }
		public decimal Price { get; set; }
		public int UserId { get; set; }
		public User User { get; set; } = null!;
	}
}
