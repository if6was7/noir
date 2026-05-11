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

		public async Task OnGetAsync()
		{
			AuctionLots = await _context.Listings
				.Include(l => l.Seller)
				.Where(l => l.IsAuction && l.Status == "active" && !l.IsRemoved)
				.OrderBy(l => l.Id)
				.ToListAsync();
		}
	}
}
