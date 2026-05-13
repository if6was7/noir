namespace noir.Models
{
	public class SavedCard
	{
		public int Id { get; set; }
		public string Network { get; set; } = string.Empty; // visa, mastercard, mir, paypal, applepay
		public string Last4 { get; set; } = string.Empty;
		public string Expiry { get; set; } = string.Empty;
		public int UserId { get; set; }
		public User User { get; set; } = null!;
	}
}