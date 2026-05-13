using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class ProductModel : PageModel
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
		public ProductModel(NoirDbContext context)
		{
			_context = context;
		}

		public Listing Listing { get; set; } = null!;
		public List<Listing> Recommendations { get; set; } = new();
		public User? CurrentUser { get; set; }
		public string ActionText { get; set; } = "Purchase Object";
		public bool SellerIsPlus { get; set; }
		public bool SellerIsAdmin { get; set; }
		public bool IsAuctionActive { get; set; }
		public string TimeRemainingText { get; set; } = string.Empty;
		public string[] Cats { get; set; } = Array.Empty<string>();
		public bool IsLiked { get; set; }
		public bool IsSaved { get; set; }

		public async Task<IActionResult> OnGetAsync(int? id)
		{
			var listingId = id ?? 1;

			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId.HasValue)
			{
				CurrentUser = await _context.Users.FindAsync(userId.Value);
				IsLiked = await _context.UserLikes.AnyAsync(ul => ul.UserId == userId.Value && ul.ListingId == listingId);
				IsSaved = await _context.UserSaves.AnyAsync(us => us.UserId == userId.Value && us.ListingId == listingId);
			}

			Listing = await _context.Listings
				.Include(l => l.Seller)
				.Include(l => l.AuctionLot)
				.Include(l => l.Reviews)
				.ThenInclude(r => r.Reviewer)
				.FirstOrDefaultAsync(l => l.Id == listingId);

			if (Listing == null) return RedirectToPage("/Index");

			Cats = string.IsNullOrEmpty(Listing.Tags)
				? Array.Empty<string>()
				: Listing.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);

			SellerIsAdmin = Listing.Seller.Role == "admin" || Listing.Seller.Role == "superadmin";
			SellerIsPlus = SellerIsAdmin || Listing.Seller.HasPlus;

			ActionText = GetActionText(Listing.Price, Listing.IsAuction, Listing.Title);

			if (Listing.IsAuction && Listing.AuctionLot != null)
			{
				IsAuctionActive = !Listing.AuctionLot.IsEnded && Listing.AuctionLot.EndDate > DateTime.UtcNow;
				if (IsAuctionActive)
				{
					var remaining = Listing.AuctionLot.EndDate - DateTime.UtcNow;
					TimeRemainingText = $"{remaining.Days}d {remaining.Hours}h {remaining.Minutes}m";
				}
			}

			Recommendations = await _context.Listings
				.Include(l => l.Seller)
				.Where(l => l.Id != listingId && !l.IsAuction && l.Status == "active" && !l.IsRemoved)
				.OrderBy(r => EF.Functions.Random())
				.Take(3)
				.ToListAsync();

			return Page();
		}

		public async Task<IActionResult> OnPostToggleLikeAsync(int id)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return Unauthorized();

			var existing = await _context.UserLikes
				.FirstOrDefaultAsync(ul => ul.UserId == userId.Value && ul.ListingId == id);

			bool liked;
			if (existing != null)
			{
				_context.UserLikes.Remove(existing);
				liked = false;
			}
			else
			{
				_context.UserLikes.Add(new UserLike { UserId = userId.Value, ListingId = id });
				liked = true;
			}

			await _context.SaveChangesAsync();
			return new JsonResult(new { success = true, liked = liked });
		}

		public async Task<IActionResult> OnPostToggleSaveAsync(int id)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return Unauthorized();

			var existing = await _context.UserSaves
				.FirstOrDefaultAsync(us => us.UserId == userId.Value && us.ListingId == id);

			bool saved;
			if (existing != null)
			{
				_context.UserSaves.Remove(existing);
				saved = false;
			}
			else
			{
				_context.UserSaves.Add(new UserSave { UserId = userId.Value, ListingId = id });
				saved = true;
			}

			await _context.SaveChangesAsync();
			return new JsonResult(new { success = true, saved = saved });
		}

		private static string GetActionText(decimal price, bool isAuction, string title)
		{
			if (isAuction) return "Place a Bid";
			if (title.ToLower() == "the celestial sphere") return "Touch the Sun";
			if (price < 500) return "Purchase Object";
			if (price < 1000) return "Acquire Object";
			if (price < 2000) return "Buy Niche";
			if (price < 5000) return "Private Selection";
			if (price < 10000) return "Investment Grade";
			if (price < 100000) return "Have Fun";
			return "Treat Yourself";
		}
	}
}