using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class ArchiveModel : PageModel
	{
		private readonly NoirDbContext _context;

		public ArchiveModel(NoirDbContext context)
		{
			_context = context;
		}

		public List<Listing> ArchivedItems { get; set; } = new();
		public User? CurrentUser { get; set; }          // ← ДОБАВЛЕНО

		public async Task OnGetAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId.HasValue)
				CurrentUser = await _context.Users.FindAsync(userId.Value);

			ArchivedItems = await _context.Listings
				.Include(l => l.Seller)
				.Include(l => l.Purchases)
				.ThenInclude(p => p.Buyer)
				.Where(l => l.Status == "sold" || l.IsRemoved || (l.IsAuction && l.AuctionLot != null && l.AuctionLot.IsEnded))
				.OrderByDescending(l => l.CreatedAt)
				.ToListAsync();
		}

		public IActionResult OnGetRedirectToSold(int id)
		{
			return RedirectToPage("/Sold", new { id = id });
		}
	}
}