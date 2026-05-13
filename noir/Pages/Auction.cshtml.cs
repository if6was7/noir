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