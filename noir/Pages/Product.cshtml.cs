using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class ProductModel : PageModel
	{
		private readonly NoirDbContext _context;

		public ProductModel(NoirDbContext context)
		{
			_context = context;
		}

		public Listing Listing { get; set; } = null!;
		public List<Listing> Recommendations { get; set; } = new();
		public string ActionText { get; set; } = "Purchase Object";
		public bool SellerIsPlus { get; set; }
		public bool SellerIsAdmin { get; set; }

		public async Task<IActionResult> OnGetAsync(int? id)
		{
			var listingId = id ?? 1;

			Listing = await _context.Listings
				.Include(l => l.Seller)
				.Include(l => l.AuctionLot)
				.FirstOrDefaultAsync(l => l.Id == listingId);

			if (Listing == null)
				return RedirectToPage("/Index");

			SellerIsAdmin = Listing.Seller.Role == "admin";
			SellerIsPlus = SellerIsAdmin || Listing.Seller.HasPlus;

			ActionText = GetActionText(Listing.Price, Listing.IsAuction, Listing.Title);

			Recommendations = await _context.Listings
				.Include(l => l.Seller)
				.Where(l => l.Id != listingId && !l.IsAuction && l.Status == "active" && !l.IsRemoved)
				.OrderBy(r => EF.Functions.Random())
				.Take(3)
				.ToListAsync();

			return Page();
		}

		private static string GetActionText(decimal price, bool isAuction, string title)
		{
			if (isAuction) return "Place a Bid";
			if (title == "the celestial sphere") return "touch the sun";
			if (price < 500) return "Purchase Object";
			if (price < 1000) return "Acquire Object";
			if (price < 2000) return "Buy Niche";
			if (price < 5000) return "Private Selection";
			if (price < 10000) return "Investment Grade";
			if (price < 100000) return "Have Fun";
			return "Treat Yourself";
		}
	}
}
