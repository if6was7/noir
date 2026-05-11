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

		public async Task<IActionResult> OnGetAsync(int? id)
		{
			var itemId = id ?? 1;
			Item = await _context.Listings
				.Include(l => l.Seller)
				.FirstOrDefaultAsync(l => l.Id == itemId && !l.IsAuction);

			if (Item == null)
				return RedirectToPage("/Index");

			HandlingFee = Item.Price * 0.02m;
			Commission = Item.Price * 0.03m;
			TotalPrice = Item.Price + HandlingFee + Commission;

			return Page();
		}
	}
}
