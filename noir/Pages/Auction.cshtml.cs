using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace noir.Pages
{
	public class AuctionModel : PageModel
	{
		public List<AuctionItem> AuctionLots { get; set; } = new();

		public void OnGet()
		{
			AuctionLots = new List<AuctionItem>
			{
				new AuctionItem { Id = 10, Title = "ancient marble bust", Price = 85000 },
				new AuctionItem { Id = 11, Title = "imperial jade scepter", Price = 120000 },
				new AuctionItem { Id = 12, Title = "black diamond signet", Price = 45000 },
				new AuctionItem { Id = 13, Title = "gold-plated telescope (18th c.)", Price = 210000 },
				new AuctionItem { Id = 14, Title = "viking iron sword", Price = 32000 }
			};
		}
	}

	public class AuctionItem
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public decimal Price { get; set; }
	}
}