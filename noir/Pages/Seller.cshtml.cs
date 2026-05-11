using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class SellerModel : PageModel
	{
		private readonly NoirDbContext _context;

		public SellerModel(NoirDbContext context)
		{
			_context = context;
		}

		public User Seller { get; set; } = null!;
		public List<Listing> SellerListings { get; set; } = new();
		public int ListingCount => SellerListings.Count;
		public bool IsOwnProfile { get; set; }
		public bool IsAdmin { get; set; }

		public async Task<IActionResult> OnGetAsync(string username)
		{
			if (string.IsNullOrWhiteSpace(username))
				return RedirectToPage("/Account");

			Seller = await _context.Users
				.Include(u => u.Listings)
				.ThenInclude(l => l.AuctionLot)
				.FirstOrDefaultAsync(u => u.Username == username);

			if (Seller == null)
				return RedirectToPage("/Index");

			SellerListings = Seller.Listings
				.Where(l => !l.IsRemoved && l.Status != "sold")
				.ToList();

			var currentUserId = HttpContext.Session.GetInt32("UserId");
			if (currentUserId.HasValue)
			{
				var currentUser = await _context.Users.FindAsync(currentUserId.Value);
				if (currentUser != null)
				{
					IsOwnProfile = currentUser.Username == username;
					IsAdmin = currentUser.Role == "admin";
				}
			}

			return Page();
		}
	}
}