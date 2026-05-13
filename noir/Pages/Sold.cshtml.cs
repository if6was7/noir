using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class SoldModel : PageModel
	{
		private readonly NoirDbContext _context;

		public SoldModel(NoirDbContext context)
		{
			_context = context;
		}

		public Listing? Listing { get; set; }
		public Purchase? Purchase { get; set; }
		public Bid? WinningBid { get; set; }
		public User? CurrentUser { get; set; }          // ← ДОБАВЛЕНО

		public async Task<IActionResult> OnGetAsync(int? id)
		{
			if (!id.HasValue) return RedirectToPage("/Index");

			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId.HasValue)
				CurrentUser = await _context.Users.FindAsync(userId.Value);

			Listing = await _context.Listings
				.Include(l => l.Seller)
				.Include(l => l.AuctionLot)
				.ThenInclude(a => a!.Bids)
				.ThenInclude(b => b.User)
				.FirstOrDefaultAsync(l => l.Id == id.Value);

			if (Listing == null) return RedirectToPage("/Index");

			Purchase = await _context.Purchases
				.Include(p => p.Buyer)
				.FirstOrDefaultAsync(p => p.ListingId == id.Value);

			if (Listing.IsAuction && Listing.AuctionLot != null)
			{
				WinningBid = await _context.Bids
					.Include(b => b.User)
					.Where(b => b.LotId == Listing.AuctionLot.Id)
					.OrderByDescending(b => b.Amount)
					.FirstOrDefaultAsync();
			}

			return Page();
		}
	}
}