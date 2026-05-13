using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using noir.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class UploadModel : PageModel
	{
		private readonly NoirDbContext _context;

		public UploadModel(NoirDbContext context)
		{
			_context = context;
		}

		[BindProperty] public string Title { get; set; } = "";
		[BindProperty] public string Description { get; set; } = "";
		[BindProperty] public decimal Price { get; set; }
		[BindProperty] public bool IsAuction { get; set; }
		[BindProperty] public string Tags { get; set; } = "";
		[BindProperty] public DateTime? AuctionEndDate { get; set; }
		[BindProperty] public decimal? StartPrice { get; set; }
		public User? CurrentUser { get; set; }          // ← ДОБАВЛЕНО

		public async Task<IActionResult> OnGetAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return RedirectToPage("/Log_In");

			CurrentUser = await _context.Users.FindAsync(userId.Value); // ← ДОБАВЛЕНО
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return Unauthorized();

			var user = await _context.Users.FindAsync(userId.Value);
			if (user == null) return Unauthorized();

			var listing = new Listing
			{
				Title = Title,
				Description = Description,
				Price = Price,
				IsAuction = IsAuction,
				Tags = Tags,
				SellerId = user.Id,
				Status = "active",
				CreatedAt = DateTime.UtcNow
			};

			_context.Listings.Add(listing);
			await _context.SaveChangesAsync();

			listing.ArchiveRef = listing.Id.ToString("D3");
			await _context.SaveChangesAsync();

			if (IsAuction && AuctionEndDate.HasValue && StartPrice.HasValue)
			{
				var auctionLot = new AuctionLot
				{
					ListingId = listing.Id,
					StartPrice = StartPrice.Value,
					CurrentPrice = StartPrice.Value,
					EndDate = AuctionEndDate.Value,
					IsEnded = false
				};
				_context.AuctionLots.Add(auctionLot);
				await _context.SaveChangesAsync();
			}

			var files = Request.Form.Files;
			if (files != null && files.Count > 0)
			{
				var photoDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "photo", listing.Id.ToString());
				if (!Directory.Exists(photoDir)) Directory.CreateDirectory(photoDir);

				for (int i = 0; i < files.Count; i++)
				{
					var file = files[i];
					if (file.Length > 0 && file.ContentType.StartsWith("image/"))
					{
						var ext = Path.GetExtension(file.FileName).ToLower();
						if (string.IsNullOrEmpty(ext)) ext = ".jpg";

						var fileName = $"{i}{ext}";
						var filePath = Path.Combine(photoDir, fileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							await file.CopyToAsync(stream);
						}

						if (i == 0)
							listing.PhotoUrl = $"/images/photo/{listing.Id}/{fileName}";
					}
				}
				await _context.SaveChangesAsync();
			}

			return RedirectToPage("/Index");
		}
	}
}