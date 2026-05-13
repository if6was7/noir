using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class IndexModel : PageModel
	{
		private readonly NoirDbContext _context;

		public IndexModel(NoirDbContext context)
		{
			_context = context;
		}
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

		public List<Listing> Listings { get; set; } = new();
		public User? CurrentUser { get; set; }

		public async Task OnGetAsync()
		{
			Listings = await _context.Listings
				.Include(l => l.Seller)
				.Include(l => l.AuctionLot)
				.Where(l => !l.IsRemoved && l.Status == "active" && !l.IsAuction)
				.OrderByDescending(l => l.CreatedAt)
				.ToListAsync();

			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId.HasValue)
				CurrentUser = await _context.Users.FindAsync(userId.Value);
		}
	}
}
