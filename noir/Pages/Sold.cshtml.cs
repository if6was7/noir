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

		public async Task<IActionResult> OnGetAsync(int? id)
		{
			if (!id.HasValue)
				return RedirectToPage("/Index");

			Listing = await _context.Listings
				.Include(l => l.Seller)
				.FirstOrDefaultAsync(l => l.Id == id.Value);

			if (Listing == null)
				return RedirectToPage("/Index");

			Purchase = await _context.Purchases
				.Include(p => p.Buyer)
				.FirstOrDefaultAsync(p => p.ListingId == id.Value);

			return Page();
		}
	}
}