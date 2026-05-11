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

		public List<Listing> Listings { get; set; } = new();
		public User? CurrentUser { get; set; }

		public async Task OnGetAsync()
		{
			Listings = await _context.Listings
				.Include(l => l.Seller)
				.Where(l => !l.IsRemoved && l.Status != "sold")
				.OrderBy(l => l.Id)
				.ToListAsync();

			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId.HasValue)
				CurrentUser = await _context.Users.FindAsync(userId.Value);
		}
	}
}