namespace noir.Models
{
	public class Review
	{
		public int Id { get; set; }
		public int Stars { get; set; }
		public string Body { get; set; } = string.Empty;
		public int ListingId { get; set; }
		public Listing Listing { get; set; } = null!;
		public int ReviewerId { get; set; }
		public User Reviewer { get; set; } = null!;
	}
}
