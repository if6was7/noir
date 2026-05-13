using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class AuctionModel : PageModel
	{
		private readonly NoirDbContext _context;
		public async Task<IActionResult> OnPostRemoveAsync(int id)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return Unauthorized();

			var user = await _context.Users.FindAsync(userId.Value);
			if (user == null || (user.Role != "admin" && user.Role != "superadmin"))
				return Forbid();

			var listing = await _context.Listings.FindAsync(id);
			if (listing == null) return NotFound();

			listing.IsRemoved = true;
			await _context.SaveChangesAsync();

			return new JsonResult(new { success = true });
		}
		public AuctionModel(NoirDbContext context)
		{
			_context = context;
		}

		public List<Listing> AuctionLots { get; set; } = new();
		public User? CurrentUser { get; set; }

		public async Task OnGetAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId.HasValue)
				CurrentUser = await _context.Users.FindAsync(userId.Value);

			AuctionLots = await _context.Listings
				.Include(l => l.Seller)
				.Include(l => l.AuctionLot)
				.Where(l => l.IsAuction && l.Status == "active" && !l.IsRemoved && l.AuctionLot != null && !l.AuctionLot.IsEnded && l.AuctionLot.EndDate > DateTime.UtcNow)
				.OrderBy(l => l.AuctionLot!.EndDate)
				.ToListAsync();
		}
	}
}