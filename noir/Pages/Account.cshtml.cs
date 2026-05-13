using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class AccountModel : PageModel
	{
		private readonly NoirDbContext _context;

		public AccountModel(NoirDbContext context)
		{
			_context = context;
		}

		public User? UserProfile { get; set; }
		public User? CurrentUser => UserProfile;

		public List<Listing> ActiveListings => UserProfile?.Listings?.Where(l => !l.IsRemoved).ToList() ?? new List<Listing>();
		public List<Listing> LikedListings { get; set; } = new();
		public List<Listing> SavedListings { get; set; } = new();
		public List<Purchase> MyPurchases { get; set; } = new();

		[BindProperty] public IFormFile? AvatarFile { get; set; }
		[BindProperty] public IFormFile? BannerFile { get; set; }
		[BindProperty] public string? UploadType { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return RedirectToPage("/Log_In");

			UserProfile = await _context.Users
				.Include(u => u.SavedCards)
				.Include(u => u.Subscription)
				.Include(u => u.Listings)
				.ThenInclude(l => l.AuctionLot)
				.Include(u => u.Purchases)
				.ThenInclude(p => p.Listing)
				.ThenInclude(l => l!.Seller)
				.Include(u => u.Likes)
				.ThenInclude(l => l.Listing)
				.ThenInclude(l => l.Seller)
				.Include(u => u.Saves)
				.ThenInclude(s => s.Listing)
				.ThenInclude(l => l.Seller)
				.FirstOrDefaultAsync(u => u.Id == userId.Value);

			if (UserProfile == null)
			{
				HttpContext.Session.Clear();
				return RedirectToPage("/Log_In");
			}

			LikedListings = UserProfile.Likes?.Select(l => l.Listing).ToList() ?? new List<Listing>();
			SavedListings = UserProfile.Saves?.Select(s => s.Listing).ToList() ?? new List<Listing>();
			MyPurchases = UserProfile.Purchases?.ToList() ?? new List<Purchase>();

			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return Unauthorized();

			var user = await _context.Users.FindAsync(userId.Value);
			if (user == null) return NotFound();

			if (UploadType == "avatar" && AvatarFile != null)
			{
				var file = AvatarFile;
				var isVideo = file.ContentType.StartsWith("video/");
				var isGif = file.ContentType == "image/gif";
				var isImage = file.ContentType.StartsWith("image/");

				if (!isVideo && !isGif && !isImage)
					return RedirectToPage();

				var canUseMedia = user.HasPlus || user.Role == "admin" || user.Role == "superadmin";
				if ((isVideo || isGif) && !canUseMedia)
					return RedirectToPage();

				if (file.Length > 10 * 1024 * 1024)
					return RedirectToPage();

				var extension = isVideo ? ".mp4" : (isGif ? ".gif" : ".jpg");
				var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatar");
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

				foreach (var ext in new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4" })
				{
					var oldFile = Path.Combine(dir, user.Username + ext);
					if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
				}

				var filePath = Path.Combine(dir, user.Username + extension);
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				user.AvatarUrl = $"/images/avatar/{user.Username}{extension}";
				await _context.SaveChangesAsync();
			}
			else if (UploadType == "banner" && BannerFile != null)
			{
				var file = BannerFile;
				if (!file.ContentType.StartsWith("image/") || file.Length > 8 * 1024 * 1024)
					return RedirectToPage();

				var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "banner");
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

				var filePath = Path.Combine(dir, user.Username + "_banner.jpg");
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				user.BannerUrl = $"/images/banner/{user.Username}_banner.jpg";
				await _context.SaveChangesAsync();
			}

			return RedirectToPage();
		}

		public async Task<IActionResult> OnPostDeleteCardAsync(int cardId)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return Unauthorized();

			var card = await _context.SavedCards.FindAsync(cardId);
			if (card == null || card.UserId != userId.Value) return NotFound();

			_context.SavedCards.Remove(card);
			await _context.SaveChangesAsync();
			return RedirectToPage();
		}

		public async Task<IActionResult> OnPostRemoveListingAsync(int listingId)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return Unauthorized();

			var listing = await _context.Listings.FindAsync(listingId);
			if (listing == null || listing.SellerId != userId.Value) return NotFound();

			listing.IsRemoved = true;
			listing.Status = "removed";
			await _context.SaveChangesAsync();
			return RedirectToPage();
		}
	}
}