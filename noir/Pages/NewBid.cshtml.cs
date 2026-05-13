using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System.Linq;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class NewBidModel : PageModel
	{
		private readonly NoirDbContext _context;

		public NewBidModel(NoirDbContext context)
		{
			_context = context;
		}

		public Listing? Item { get; set; }
		public decimal TotalPrice { get; set; }
		public decimal HandlingFee { get; set; }
		public decimal Commission { get; set; }
		public decimal MinBid { get; set; }
		public User? CurrentUser { get; set; }

		public async Task<IActionResult> OnGetAsync(int? id)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return RedirectToPage("/Log_In");

			CurrentUser = await _context.Users.FindAsync(userId.Value);
			if (CurrentUser == null) return RedirectToPage("/Log_In");

			var itemId = id ?? 1;
			Item = await _context.Listings
				.Include(l => l.Seller)
				.Include(l => l.AuctionLot)
				.FirstOrDefaultAsync(l => l.Id == itemId && l.IsAuction && l.Status == "active");

			if (Item == null || Item.AuctionLot == null || Item.AuctionLot.IsEnded)
				return RedirectToPage("/Index");

			HandlingFee = Item.Price * 0.02m;
			Commission = Item.Price * 0.03m;
			TotalPrice = Item.Price + HandlingFee + Commission;

			// FIX: учитываем MinBidStep вместо фиксированных 0.01
			MinBid = Item.AuctionLot.CurrentPrice + Item.AuctionLot.MinBidStep;

			return Page();
		}
	}
}