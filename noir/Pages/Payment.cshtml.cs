using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using noir.Models;
using System;
using System.Threading.Tasks;

namespace noir.Pages
{
	public class PaymentModel : PageModel
	{
		private readonly NoirDbContext _context;

		public PaymentModel(NoirDbContext context)
		{
			_context = context;
		}

		[BindProperty(SupportsGet = true)] public string Source { get; set; } = "subscription";
		[BindProperty(SupportsGet = true)] public string Method { get; set; } = "visa";
		[BindProperty(SupportsGet = true)] public decimal Amount { get; set; }
		[BindProperty(SupportsGet = true)] public int? LotId { get; set; }
		[BindProperty(SupportsGet = true)] public string ItemTitle { get; set; } = "";
		[BindProperty(SupportsGet = true)] public bool UseBalance { get; set; } = false;
		[BindProperty] public string CardNumber { get; set; } = "";
		[BindProperty] public string CardHolder { get; set; } = "";
		[BindProperty] public string CardExpiry { get; set; } = "";
		[BindProperty] public string CardCvv { get; set; } = "";

		public User? CurrentUser { get; set; }
		public bool IsLinking => Source == "linkcard";
		public bool IsTopup => Source == "topup";
		public bool IsAuction => Source == "auction";
		public bool IsPurchase => Source == "purchase";
		public bool IsSubscription => !IsLinking && !IsTopup && !IsAuction && !IsPurchase;

		public async Task<IActionResult> OnGetAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return RedirectToPage("/Log_In");

			CurrentUser = await _context.Users.FindAsync(userId.Value);
			if (CurrentUser == null) return RedirectToPage("/Log_In");

			// FIX: принудительно ставим цену подписки, если не передана
			if (IsSubscription && Amount == 0)
				Amount = 10.99m;

			return Page();
		}

		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> OnPostAsync()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (!userId.HasValue) return Unauthorized();

			var user = await _context.Users.FindAsync(userId.Value);
			if (user == null) return Unauthorized();

			try
			{
				if (IsLinking && !string.IsNullOrEmpty(CardNumber) && CardNumber.Length >= 4)
				{
					var last4 = CardNumber.Replace(" ", "").Replace("-", "");
					last4 = last4.Length > 4 ? last4[^4..] : last4;

					_context.SavedCards.Add(new SavedCard
					{
						UserId = user.Id,
						Network = Method,
						Last4 = last4,
						Expiry = CardExpiry
					});
				}
				else if (IsTopup && Amount > 0)
				{
					user.Balance += Amount;
				}
				else if (IsSubscription)
				{
					decimal subPrice = 10.99m;

					// FIX: списание с баланса для подписки
					if (UseBalance)
					{
						if (user.Balance < subPrice)
							return new JsonResult(new { success = false, error = "Insufficient balance" });
						user.Balance -= subPrice;
					}

					user.HasPlus = true;
					_context.Subscriptions.Add(new Subscription
					{
						UserId = user.Id,
						IsActive = true,
						ActiveSince = DateTime.UtcNow,
						EndDate = DateTime.UtcNow.AddMonths(1),
						Price = subPrice
					});
				}
				else if (IsAuction && LotId.HasValue && Amount > 0)
				{
					var lot = await _context.AuctionLots
						.Include(a => a.Listing)
						.FirstOrDefaultAsync(a => a.ListingId == LotId.Value);

					if (lot == null || lot.IsEnded || lot.EndDate <= DateTime.UtcNow)
						return new JsonResult(new { success = false, error = "Auction ended or not found" });

					if (UseBalance)
					{
						if (user.Balance < Amount)
							return new JsonResult(new { success = false, error = "Insufficient balance" });
						user.Balance -= Amount;
					}

					if (Amount > lot.CurrentPrice)
					{
						lot.CurrentPrice = Amount;
						_context.Bids.Add(new Bid
						{
							LotId = lot.Id,
							UserId = user.Id,
							Amount = Amount,
							CreatedAt = DateTime.UtcNow
						});
					}
					else
					{
						return new JsonResult(new { success = false, error = "Bid must be higher than current price" });
					}
				}
				else if (IsPurchase && LotId.HasValue && Amount > 0)
				{
					var listing = await _context.Listings.FindAsync(LotId.Value);
					if (listing == null || listing.Status != "active")
						return new JsonResult(new { success = false, error = "Item not available" });

					if (UseBalance)
					{
						if (user.Balance < Amount)
							return new JsonResult(new { success = false, error = "Insufficient balance" });
						user.Balance -= Amount;
					}

					listing.Status = "sold";
					_context.Purchases.Add(new Purchase
					{
						ListingId = LotId.Value,
						BuyerId = user.Id,
						Amount = Amount,
						CreatedAt = DateTime.UtcNow
					});
				}

				await _context.SaveChangesAsync();
				return new JsonResult(new { success = true });
			}
			catch (Exception ex)
			{
				return new JsonResult(new { success = false, error = ex.Message });
			}
		}
	}
}